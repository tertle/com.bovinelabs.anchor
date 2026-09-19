"""Summarize five-run particle captures; optional second directory compares matched runs."""
import csv
import json
import math
import statistics
import sys
from collections import defaultdict
from pathlib import Path


def summarize(directory):
    directory = Path(directory)
    if not (directory / "runtime-complete.txt").exists():
        raise ValueError("Incomplete Player capture")
    result = {}
    for path in sorted(directory.glob("*-frames.csv")):
        runs = defaultdict(list)
        with path.open(newline="", encoding="utf-8-sig") as stream:
            for row in csv.DictReader(stream):
                particle_markers = [float(row["Anchor.Particles." + marker]) for marker in
                                    ("Simulation", "Appearance", "MeshFill", "MeshAllocate", "MeshSubmit")]
                # These disjoint main-thread scopes exclude coordinator/visibility work and Unity's later processing.
                row["particleNativePathNs"] = sum(particle_markers) if min(particle_markers) >= 0 else -1
                runs[(row["mode"], int(row["run"]))].append(row)
        modes = defaultdict(list)
        for (mode, run), rows in runs.items():
            if len(rows) != 600:
                raise ValueError(f"Incomplete {path.name}/{mode}/{run}")
            metrics = {}
            for key in rows[0]:
                if key in ("mode", "run", "frame"):
                    continue
                values = sorted(float(row[key]) for row in rows if float(row[key]) >= 0)
                metrics[key] = None if not values else {
                    "median": statistics.median(values), "p95": values[math.ceil(len(values) * .95) - 1],
                    "max": values[-1], "min": values[0], "samples": len(values),
                }
            modes[mode].append(metrics)
        for mode, samples in modes.items():
            if len(samples) != 5:
                raise ValueError(f"Expected five runs: {path.name}/{mode}")
            metrics = {}
            for key in samples[0]:
                valid = [sample[key] for sample in samples if sample[key] is not None]
                metrics[key] = None if len(valid) != 5 else {
                    "median": statistics.median(v["median"] for v in valid),
                    "medianNoise": max(v["median"] for v in valid) - min(v["median"] for v in valid),
                    "p95": statistics.median(v["p95"] for v in valid),
                    "p95Noise": max(v["p95"] for v in valid) - min(v["p95"] for v in valid),
                    "max": max(v["max"] for v in valid), "min": min(v["min"] for v in valid),
                    "runs": valid,
                }
            result[path.stem.removesuffix("-frames") + "/" + mode] = metrics
    if not result:
        raise ValueError("No measured workloads")
    (directory / "runtime-summary.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    return result


before = summarize(sys.argv[1])
if len(sys.argv) > 2:
    after = summarize(sys.argv[2])
    if before.keys() != after.keys():
        raise ValueError("Workloads differ")
    comparisons = {}
    for workload, metrics in before.items():
        changes = {}
        for name, base in metrics.items():
            candidate = after[workload][name]
            if base is None or candidate is None:
                continue
            improvement = base["median"] - candidate["median"]
            changes[name] = {
                "before": base["median"], "after": candidate["median"], "noise": base["medianNoise"],
                "improvementExceedsGate": improvement >= base["median"] * .1 and improvement > 2 * base["medianNoise"],
                "medianRegression": candidate["median"] - base["median"] > max(.05 * base["median"], 2 * base["medianNoise"]),
                "p95Regression": candidate["p95"] - base["p95"] > max(.05 * base["p95"], 2 * base["p95Noise"]),
            }
        comparisons[workload] = changes
    (Path(sys.argv[2]) / "comparison.json").write_text(json.dumps(comparisons, indent=2), encoding="utf-8")
print(f"Summarized {len(before)} workloads; unavailable counters remain null; noise is full repeat-run spread.")
