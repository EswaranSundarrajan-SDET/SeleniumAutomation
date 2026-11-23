import sys
import xml.etree.ElementTree as ET
from datetime import datetime


def peak_concurrency(results):
    events = []
    for result in results:
        start = datetime.fromisoformat(result.attrib["startTime"])
        end = datetime.fromisoformat(result.attrib["endTime"])
        if start < end:
            events.extend([(start, 1), (end, -1)])

    active = peak = 0
    for _, change in sorted(events):
        active += change
        peak = max(peak, active)
    return peak


def main():
    path, minimum_passed = sys.argv[1:]
    trx = ET.parse(path)
    counters = trx.find(".//{*}Counters")
    passed = int(counters.get("passed", "0")) if counters is not None else 0
    if passed < int(minimum_passed):
        raise SystemExit(f"Expected at least {minimum_passed} passing scenarios, found {passed}")

    results = trx.findall(".//{*}Results/{*}UnitTestResult")
    peak = peak_concurrency(result for result in results if result.get("outcome") == "Passed")
    print(f"{passed} scenarios passed. Peak concurrent scenarios: {peak}.")
    if peak < 2:
        raise SystemExit("Expected at least two scenarios to overlap")


if __name__ == "__main__":
    main()
