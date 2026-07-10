# UCSD_DataMiningHighMod

One-off data-mining analysis that identifies external-beam radiotherapy plans with high
modulation factors from locally exported ARIA patient databases. Not a maintained tool;
paths are hardcoded for the author's environment.

## How it works

Everything lives in `FindingHighModulationPatients/FindModulationPatients.cs`:

1. Loads patient JSON files exported from ARIA (read with `AriaDataBaseJsonReader` from
   the sibling `Make_Raystation_Data_StructureCSharp` solution: `DataBaseStructure`,
   `DataBaseFileManager`, `DataWritingTools`).
2. Walks each patient's courses and keeps only treatment-approved external-beam plans.
3. For each beam set, sums monitor units over beams with MU > 0 and computes a
   modulation factor = total MU / prescribed dose per fraction (highest prescription
   target dose).
4. Writes `ModulationPatients.csv` with MRN, plan review date, course, plan name, dose
   per fraction, fraction count, total dose, beam count, modulation factor, and
   delivery technique.

## Requirements

- .NET Framework 4.8; Newtonsoft.Json 13.
- Builds as a class library and references three projects from a sibling
  `Make_Raystation_Data_StructureCSharp` checkout, so it does not build standalone.
- Input JSON directories and the output CSV path are hardcoded in `Main` (local
  database folders and a UCSD network share); edit them before running.
