# Phase 4B - Timing Causality Analysis

- source log: `Phase4b\matrix-2\phase4b-driver.log`
- stopwatch frequency: 10000000 Hz (ticks == 100ns units when freq=10MHz)
- runs analyzed: 18

## Per-run stage metrics (ms)

| run | delay | armDelayActual | content@feed | feed(T0+) | 1stFeed(pre-) | feedTick | lagP50 | lagP95 | PTS identity | MP4 pts/start/pkts | pass |
|---|---|---|---|---|---|---|---|---|---|---|---|
| arm-off1 | 0 | 0 | 38 | 50.2 | **50.2** | 4 | 7.4 | 15.4 | True | 0.000000/0.000000/180 | True |
| arm-off2 | 0 | 0 | 50 | 51.3 | **51.3** | 4 | 7.4 | 15.8 | True | 0.000000/0.000000/180 | True |
| arm-off3 | 0 | 0 | 35.2 | 50.4 | **50.4** | 4 | 8.1 | 15.8 | True | 0.000000/0.000000/180 | True |
| arm-on1 | 100 | 100 | 97.1 | 0.6 | **100.7** | 1 | 7.6 | 15.8 | True | 0.000000/0.000000/181 | True |
| arm-on2 | 100 | 100 | 92.4 | 0.2 | **100.2** | 1 | 8.5 | 15.8 | True | 0.000000/0.000000/181 | True |
| arm-on3 | 100 | 100 | 99.2 | 1.7 | **101.7** | 1 | 8.5 | 15.6 | True | 0.000000/0.000000/181 | True |
| arm-on101 | 10 | 10 | 34.7 | 34.4 | **44.5** | 3 | 8.5 | 15.6 | True | 0.000000/0.000000/180 | True |
| arm-on102 | 10 | 10 | 34 | 33.7 | **43.7** | 3 | 8 | 15.3 | True | 0.000000/0.000000/180 | True |
| arm-on103 | 10 | 10 | 35.9 | 33.5 | **43.5** | 3 | 8.2 | 15.8 | True | 0.000000/0.000000/180 | True |
| loop-off1 | 0 | 0 | 35.8 | 34 | **41.2** | 3 | 8.5 | 16.4 | True | 0.000000/0.000000/180 | True |
| loop-off2 | 0 | 0 | 33.7 | 33.6 | **39** | 3 | 8.5 | 16.1 | True | 0.000000/0.000000/180 | True |
| loop-off3 | 0 | 0 | 34 | 33.5 | **39.1** | 3 | 8 | 15.6 | True | 0.000000/0.000000/180 | True |
| loop-on1 | 100 | 100 | 97.2 | 0.4 | **105.8** | 1 | 8.6 | 15.8 | True | 0.000000/0.000000/181 | True |
| loop-on2 | 100 | 100 | 96.5 | 0.5 | **105.8** | 1 | 8.8 | 16.5 | True | 0.000000/0.000000/181 | True |
| loop-on3 | 100 | 100 | 98 | 0.3 | **106.6** | 1 | 8.8 | 16.3 | True | 0.000000/0.000000/181 | True |
| loop-on101 | 10 | 10 | 34.3 | 33.5 | **49.1** | 3 | 8.4 | 15.4 | True | 0.000000/0.000000/180 | True |
| loop-on102 | 10 | 10 | 38.5 | 34.4 | **54.2** | 3 | 8.1 | 15.5 | True | 0.000000/0.000000/180 | True |
| loop-on103 | 10 | 10 | 34.5 | 34.4 | **49.7** | 3 | 8 | 16.2 | True | 0.000000/0.000000/180 | True |

## Group medians (1stFeed = first encoded packet handed to the mux, from preRun)

| group | n | 1stFeed median (ms) | content@feed median | lagP50 | lagP95 | MP4 packets |
|---|---|---|---|---|---|---|
| arm/on | 3 | 100.7 | 97.1 | 8.5 | 15.8 | 181 |
| arm/on10 | 3 | 43.7 | 34.7 | 8.2 | 15.6 | 180 |
| arm/off | 3 | 50.4 | 38 | 7.4 | 15.8 | 180 |
| loop/on | 3 | 105.8 | 97.2 | 8.8 | 16.3 | 181 |
| loop/on10 | 3 | 49.7 | 34.5 | 8.1 | 15.5 | 180 |
| loop/off | 3 | 39.1 | 34 | 8.5 | 16.1 | 180 |

## Delta first-feed between arms (same t0-mode, medians)

- delta(on-off) [arm]: 50.3 ms  (on=100.7, off=50.4)
- delta(on10-off) [arm]: -6.7 ms  (on10=43.7, off=50.4)
- delta(on-off) [loop]: 66.8 ms  (on=105.8, off=39.1)
- delta(on10-off) [loop]: 10.6 ms  (on10=49.7, off=39.1)

## Verdicts

- **V1 timeline arm delay exact** (T0-armCall == delay within 2ms in every run): PASS
- **V2 Packet PTS identity** (packet.PTS == frame.PTS == capture time, every run): PASS
- **V3 MP4 PTS** (first video PTS = 0 and start_time = 0 in every run): PASS - container PTS is frame-index-based; the timeline delay is invisible in the MP4 by construction
- **V4 CFR grid alignment** (median selection lag <= 25ms = 1 frame at 60fps): PASS
- **V5 session pass** (mux ok, dropped=0, file valid, every run): PASS


