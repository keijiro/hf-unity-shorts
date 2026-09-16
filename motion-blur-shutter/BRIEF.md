---
workflow: general-video
flow: automation
storyboard: no
format: 2160x2160
fps: 60
duration: 40
---

# Shutter — instant sample vs. open shutter

Cut-in animation for the motion-blur explainer short. It covers the stretch of
`../../yomu/script.txt` from 「ゲームのようなリアルタイム・レンダリングは」 (line 7)
to 「つまりモーションブラーを生み出すわけです」 (line 11).

## Message

A frame is written by a shutter. Give the shutter no duration and every frame is
a crisp instant; hold it open and whatever moved during it is written into the
frame as blur.

## Structure

One file, two passes, hard cut at 20s. Both passes replay the **same** world
interval (w = 0.6 → 16.6, sixteen captures at 1.25s apart), so the only
variable on screen is the shutter.

- **Pass A — 0–20s.** Real-time rendering. The world freezes the instant a frame
  reaches the gate, the eye blinks open, one crisp sample burns in.
  Carries script lines 7–8.
- **Pass B — 20–40s.** The same motion, never frozen. The eye opens as a frame
  reaches the gate and stays open for half the frame interval (a 180° shutter);
  the streak extends in the frame for exactly as long as the eye is open. The
  film runs intermittently, like a real camera: held dead still in the gate
  while the shutter is open, pulled on while it is shut.
  Carries script lines 9–11.

## Layout

Left: the world — a ball bouncing in a box. Centre: the shutter, drawn as an eye.
Right: the film strip, scrolling up one frame per capture; below the gate the
frames are blank, above it they hold what was written. How long the eye stays
open — and how long the frame in the gate keeps its amber border — *is* the
exposure; there is no other shutter indicator.

## Notes

- No type anywhere — the narration carries every label.
- Pass B integrates the world over the shutter with 51 sub-shutter samples summed
  in `plus-lighter`. `#ff9933` is (255,153,51), so each sample contributes exactly
  (5,3,1) and the sum is bit-exact.
- Pure black ground, one accent (`#ff9933`), no type. Otherwise the same house
  style as the sibling `motion-blur-wagon-wheel` project: everything is a pure
  function of composition time.
