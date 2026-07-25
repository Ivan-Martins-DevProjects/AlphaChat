# AlphaChat — Dark Theme Palette

## Base Backgrounds

| Token | Color | Usage |
|---|---|---|
| `--bg-primary` | `#0b1220` | Main application background |
| `--bg-secondary` | `#112240` | Sidebar / elevated areas |
| `--bg-card` | `#151f32` | Cards, searchbars, containers |
| `--bg-hover` | `#1e293b` | Hover states |

---

## Accent Colors

| Token | Color | Usage |
|---|---|---|
| `--accent-primary` | `#0f766e` | Active items |
| `--accent-secondary` | `#134e4a` | Gradient support |
| `--accent-highlight` | `#5eead4` | Focus, highlights, badges |

---

## Text Colors

| Token | Color | Usage |
|---|---|---|
| `--text-primary` | `#f8fafc` | Main text |
| `--text-secondary` | `#cbd5e1` | Secondary text |
| `--text-muted` | `#94a3b8` | Placeholder / timestamps |
| `--text-disabled` | `#64748b` | Disabled states |

---

## Borders & Dividers

| Token | Color | Usage |
|---|---|---|
| `--border-primary` | `rgba(255,255,255,0.06)` | Main borders |
| `--border-secondary` | `rgba(255,255,255,0.04)` | Soft borders |
| `--divider` | `rgba(255,255,255,0.08)` | Horizontal separators |

---

## Shadows

| Token | Value | Usage |
|---|---|---|
| `--shadow-soft` | `0 10px 24px rgba(0,0,0,0.25)` | Cards hover |
| `--shadow-medium` | `0 12px 30px rgba(0,0,0,0.35)` | Main containers |
| `--shadow-glow` | `0 0 0 4px rgba(94,234,212,0.08)` | Focus states |

---

## Suggested CSS Variables

```css
:root {
  --bg-primary: #0b1220;
  --bg-secondary: #112240;
  --bg-card: #151f32;
  --bg-hover: #1e293b;

  --accent-primary: #0f766e;
  --accent-secondary: #134e4a;
  --accent-highlight: #5eead4;

  --text-primary: #f8fafc;
  --text-secondary: #cbd5e1;
  --text-muted: #94a3b8;
  --text-disabled: #64748b;

  --border-primary: rgba(255,255,255,0.06);
  --border-secondary: rgba(255,255,255,0.04);
  --divider: rgba(255,255,255,0.08);

  --shadow-soft: 0 10px 24px rgba(0,0,0,0.25);
  --shadow-medium: 0 12px 30px rgba(0,0,0,0.35);
  --shadow-glow: 0 0 0 4px rgba(94,234,212,0.08);
}
