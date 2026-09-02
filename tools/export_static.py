#!/usr/bin/env python3
import re
import shutil
import sys
import urllib.request
from pathlib import Path

BASE = "http://localhost:5204"
LINK_RE = re.compile(r'(?:href|src)="(/[^"]*)"')
SKIP_RE = re.compile(r"^/(admin|auth|api|denied|error|healthz)(/|$)")


def fetch(path: str) -> tuple[int, bytes]:
    req = urllib.request.Request(BASE + path)
    try:
        with urllib.request.urlopen(req) as resp:
            return resp.status, resp.read()
    except urllib.error.HTTPError as e:
        return e.code, e.read()


def out_file(out: Path, path: str) -> Path:
    if path == "/":
        return out / "index.html"
    rel = path.lstrip("/")
    if "." in path.rsplit("/", 1)[-1]:
        return out / rel
    return out / rel / "index.html"


def extract_links(html: str) -> set[str]:
    links = set()
    for match in LINK_RE.finditer(html):
        url = match.group(1).split("#")[0].split("?")[0]
        if url and not SKIP_RE.match(url):
            links.add(url)
    return links


def save(target: Path, body: bytes) -> None:
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(body)


def crawl(out: Path) -> int:
    queue, seen, errors = ["/"], set(), 0
    while queue:
        path = queue.pop()
        if path in seen:
            continue
        seen.add(path)
        status, body = fetch(path)
        if status != 200:
            print(f"FAIL {status} {path}")
            errors += 1
            continue
        save(out_file(out, path), body)
        if out_file(out, path).suffix == ".html":
            queue.extend(extract_links(body.decode("utf-8")) - seen)
    print(f"crawled {len(seen)} urls, {errors} errors")
    return errors


def copy_wwwroot(out: Path) -> None:
    www = Path(__file__).resolve().parent.parent / "src/LabHub.Web/wwwroot"
    for item in www.iterdir():
        dest = out / item.name
        if item.is_dir():
            shutil.copytree(item, dest, dirs_exist_ok=True)
        else:
            shutil.copy2(item, dest)


def write_extras(out: Path) -> None:
    (out / ".nojekyll").write_bytes(b"")
    status, body = fetch("/c/this-page-does-not-exist")
    if status == 404 and body:
        (out / "404.html").write_bytes(body)


PREFIX_RE = re.compile(r'((?:href|src)=")/(?!/)')


def apply_prefix(out: Path, prefix: str) -> None:
    if not prefix:
        return
    for page in out.rglob("*.html"):
        html = page.read_text(encoding="utf-8")
        page.write_text(PREFIX_RE.sub(r"\1" + prefix + "/", html), encoding="utf-8")
    print(f"prefixed internal links with {prefix}")


def main() -> int:
    out = Path(sys.argv[1] if len(sys.argv) > 1 else "static-out")
    prefix = sys.argv[2].rstrip("/") if len(sys.argv) > 2 else ""
    if out.exists():
        shutil.rmtree(out)
    out.mkdir(parents=True)
    errors = crawl(out)
    copy_wwwroot(out)
    write_extras(out)
    apply_prefix(out, prefix)
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
