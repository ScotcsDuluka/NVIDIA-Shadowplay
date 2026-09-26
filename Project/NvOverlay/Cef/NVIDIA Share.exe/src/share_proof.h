// share_proof.h - CEF lane runtime evidence: host log + proof-of-life
// checkpoints. Mirrors the mission requirement "proof-of-life: process
// start -> CEF init -> page load -> cefQuery round-trip" with QPC-timed,
// UTC-stamped checkpoints written to <exeDir>\Logs\proof-of-life.json.
#ifndef SHARE_PROOF_H_
#define SHARE_PROOF_H_

#include <string>

namespace shareproof {

// Resolves the exe dir and opens the host log. Must be the first call in
// NvShareCefMain; |proof_mode| selects whether proof checkpoints are
// recorded and proof-of-life.json is written (subprocess relaunches run
// with proof_mode=false and only log).
void Init(bool proof_mode);

// Turns proof recording on after the fact (browser-process arg handling
// happens after the initial Init; QPC timeline is preserved).
void EnableProofMode();

// Runtime log line (mutex-protected; also OutputDebugStringA).
void LogLine(const std::string& line);

// Records one checkpoint (idempotent per name: first occurrence wins).
void Checkpoint(const char* name, const std::string& detail = std::string());

// Extra proof fields. |json_value| must be a ready-to-emit JSON value
// (numbers raw, strings already escaped via sharejson::Escape).
void SetField(const char* key, const std::string& json_value);

// Writes Logs\proof-of-life.json with run info + fields + checkpoints.
void WriteProof();

// 0 when every required checkpoint is present, 2 otherwise. Always 0
// when not in proof mode.
int ExitCode();

}  // namespace shareproof

#endif  // SHARE_PROOF_H_
