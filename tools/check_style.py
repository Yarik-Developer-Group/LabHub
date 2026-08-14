#!/usr/bin/env python3
import re
import sys
from pathlib import Path

METHOD_RE = re.compile(
    r'^\s*(?:\[[^\]]*\]\s*)*'
    r'(?:public|private|protected|internal)\s+'
    r'(?:static\s+|async\s+|sealed\s+|override\s+|virtual\s+|partial\s+)*'
    r'[\w<>,\[\]?()\s.]+?\s(\w+)\s*\(([^)]*)\)\s*$'
)

def count_params(params: str) -> int:
    params = params.strip()
    if not params:
        return 0
    depth = 0
    count = 1
    for ch in params:
        if ch in '<([':
            depth += 1
        elif ch in '>)]':
            depth -= 1
        elif ch == ',' and depth == 0:
            count += 1
    return count

def body_lines(lines: list[str], start: int) -> int:
    depth = 0
    opened = False
    n = 0
    for i in range(start, len(lines)):
        line = lines[i]
        if '=>' in line and not opened and '{' not in line:
            return 1
        depth += line.count('{') - line.count('}')
        if '{' in line:
            opened = True
        if opened:
            n += 1
        if opened and depth == 0:
            return max(0, n - 2)
    return n

def main() -> int:
    root = Path(__file__).resolve().parent.parent
    violations = []
    for folder in ('src', 'tests'):
        for path in (root / folder).rglob('*.cs'):
            if 'obj' in path.parts or 'bin' in path.parts:
                continue
            lines = path.read_text(encoding='utf-8').splitlines()
            for i, line in enumerate(lines):
                m = METHOD_RE.match(line)
                if not m or line.strip().endswith(';'):
                    continue
                name, params = m.group(1), m.group(2)
                p = count_params(params)
                b = body_lines(lines, i)
                rel = path.relative_to(root)
                if p > 3:
                    violations.append(f'{rel}:{i + 1} {name}: {p} параметров (> 3)')
                if b > 30:
                    violations.append(f'{rel}:{i + 1} {name}: {b} строк тела (> 30)')
    for v in violations:
        print('VIOLATION', v)
    print(f'{"FAIL" if violations else "OK"}: {len(violations)} нарушений')
    return 1 if violations else 0

if __name__ == '__main__':
    sys.exit(main())
