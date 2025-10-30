using System;
using DataBaseStructure.AriaBase;
using DataBaseStructure;
using DataBaseFileManager;
using DataWritingTools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindingHighModulationPatients
{
    public class OutPatient
    {
        public string PatientID { get; set; }
        public string DateTreated { get; set; }
        public string CourseName { get; set; }
        public string PlanName { get; set; }
        public double DosePerFraction { get; set; }
        public int NumberOfFractions { get; set; }
        public double TotalDose { get; set; }
        public int NumberOfBeams { get; set; }
        public double ModulationFactor { get; set; }
        public string DeliveryTechnique { get; set; }
    }
    public class FindModulationPatientsClass
    {
        static List<OutPatient> FindModulationPatients(List<PatientClass> patients)
        {
            List<OutPatient> outPatients = new List<OutPatient>();
            foreach (PatientClass patient in patients)
            {
                foreach (CourseClass course in patient.Courses)
                {
                    foreach (TreatmentPlanClass planClass in course.TreatmentPlans)
                    {
                        if (planClass.Review.ApprovalStatus != "TreatmentApproved")
                        {
                            continue;
                        }
                        if (planClass.PlanType == "ExternalBeam")
                        {
                            foreach (BeamSetClass beamSet in planClass.BeamSets)
                            {
                                string deliveryTechnique = "Unknown";
                                PrescriptionClass prescription = beamSet.Prescription;
                                double dose = 0;
                                if (prescription == null)
                                {
                                    continue;
                                }
                                foreach (var target in prescription.PrescriptionTargets)
                                {
                                    if (target.DosePerFraction > dose)
                                    {
                                        dose = target.DosePerFraction;
                                    }
                                }
                                if (dose == 0)
                                {
                                    continue;
                                }
                                double monitorUnits = 0;
                                int beamCount = 0;
                                foreach(BeamClass beam in beamSet.Beams)
                                {
                                    if (beam.BeamMU <= 0)
                                    {
                                        continue;
                                    }
                                    deliveryTechnique = beam.DeliveryTechnique;
                                    beamCount++;
                                    monitorUnits += beam.BeamMU;
                                }
                                double modulationFactor = monitorUnits / (dose);
                                OutPatient outPatient = new OutPatient()
                                {
                                    PatientID = patient.MRN,
                                    DateTreated = $"{planClass.Review.ReviewTime.Month:D2}" +
    $"/{planClass.Review.ReviewTime.Day:D2}" +
    $"/{planClass.Review.ReviewTime.Year}",
                                    CourseName = course.Name,
                                    PlanName = planClass.PlanName,
                                    DosePerFraction = dose,
                                    NumberOfFractions = planClass.FractionNumber,
                                    TotalDose = dose * planClass.FractionNumber,
                                    ModulationFactor = modulationFactor,
                                    NumberOfBeams = beamCount,
                                    DeliveryTechnique = deliveryTechnique,
                                };

                                outPatients.Add(outPatient);
                                }
                            }
                        }
                    }
                }
            return outPatients;
        }
        public void Main(string[] args)
        {
            string dataDirectory = @"\\ad.ucsd.edu\ahs\CANC\RADONC\BMAnderson\DataBases";
            List<string> jsonFiles = new List<string>();
            jsonFiles = AriaDataBaseJsonReader.ReturnPatientFileNames(@"C:\Users\BRA008\Modular_Projects\LocalDatabases\2025", jsonFiles, "*.json", SearchOption.AllDirectories);
            //jsonFiles = AriaDataBaseJsonReader.ReturnPatientFileNames(@"C:\Users\BRA008\Modular_Projects\LocalDatabases\2024", jsonFiles, "*.json", SearchOption.AllDirectories);
            //jsonFiles = AriaDataBaseJsonReader.ReturnPatientFileNames(@"C:\Users\BRA008\Modular_Projects\LocalDatabases\2023", jsonFiles, "*.json", SearchOption.AllDirectories);
            //jsonFiles = AriaDataBaseJsonReader.ReturnPatientFileNames(@"C:\Users\BRA008\Modular_Projects\LocalDatabases\2022", jsonFiles, "*.json", SearchOption.AllDirectories);
            List<PatientClass> allPatients = new List<PatientClass>();
            allPatients = AriaDataBaseJsonReader.ReadPatientFiles(jsonFiles);
            var modulationPatients = FindModulationPatients(allPatients);
            string outputCsvPath = Path.Combine(dataDirectory, "ModulationPatients.csv");
            CsvTools.WriteToCsv<OutPatient>(modulationPatients, outputCsvPath);
        }
    }
}
