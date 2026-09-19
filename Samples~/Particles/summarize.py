"""Summarize raw BL-299 Player frames without subtracting the empty baseline."""
import csv
import json
import math
import statistics
import sys
from collections import defaultdict
from pathlib import Path

directory = Path(sys.argv[1]).resolve()
groups = defaultdict(list)
with (directory / "frames.csv").open(newline="", encoding="utf-8-sig") as stream:
    for row in csv.DictReader(stream):
        groups[(row["workload"], int(row["run"]))].append(row)


def percentile(values, fraction):
    return sorted(values)[max(0, math.ceil(len(values) * fraction) - 1)]


result = {}
for (workload, run), rows in groups.items():
    if len(rows) != 600:
        raise ValueError(f"Incomplete run: {workload}/{run}: {len(rows)} frames")
    measured = {}
    for column in rows[0]:
        if column in ("workload", "run", "frame"):
            continue
        values = [float(row[column]) for row in rows if float(row[column]) >= 0]
        measured[column] = None if not values else {
            "samples": len(values), "p50": statistics.median(values),
            "p95": percentile(values, 0.95), "max": max(values), "min": min(values),
        }
    result.setdefault(workload, {"runs": []})["runs"].append({"run": run, "metrics": measured})

table = ["| Workload | Frame p50 range ms | Frame p95 range ms | Worst frame ms | GPU p50 range ms |",
         "| --- | ---: | ---: | ---: | ---: |"]
for workload, data in result.items():
    if len(data["runs"]) != 5:
        raise ValueError(f"Expected five runs of {workload}")
    spread = {}
    for metric in data["runs"][0]["metrics"]:
        valid = [run["metrics"][metric] for run in data["runs"] if run["metrics"][metric] is not None]
        spread[metric] = None if not valid else {
            "p50Min": min(v["p50"] for v in valid), "p50Max": max(v["p50"] for v in valid),
            "p95Min": min(v["p95"] for v in valid), "p95Max": max(v["p95"] for v in valid),
            "max": max(v["max"] for v in valid),
        }
    data["spread"] = spread
    cpu = spread["frameMs"]
    gpu = spread["gpuMs"]
    gpu_text = "unavailable" if gpu is None else f'{gpu["p50Min"]:.3f}–{gpu["p50Max"]:.3f}'
    table.append(f'| {workload} | {cpu["p50Min"]:.3f}–{cpu["p50Max"]:.3f} | '
                 f'{cpu["p95Min"]:.3f}–{cpu["p95Max"]:.3f} | {cpu["max"]:.3f} | {gpu_text} |')

if not (directory / "complete.txt").exists():
    raise ValueError("Player did not write its completion marker")
(directory / "summary.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
(directory / "summary.md").write_text("\n".join(table) + "\n", encoding="utf-8")
print("\n".join(table))
