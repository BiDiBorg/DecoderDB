```sh
python -m venv .venv
source .venv/bin/activate
python -m pip install -r requirements.txt
```

```sh
for f in *.png *.PNG
  test -f "$f"; or continue
  magick "$f" -quality 95 (string replace -r -i '\.png$' '.webp' "$f")
end
```

Where necessary (and possible), isolate images for better Dark Mode support.

.webp settings
- 1000 max length
- 95% quality

TODO:
- /99
- /115
- /117
- /123
- /131
- /145
- /151
- /157
- /172
