namespace MedAssistNG.Api.Mcp
{
    public class ClinicalKnowledgeTool
    {
        public string GetCommonCauses(string symptoms)
        {
            var s = symptoms.ToLower();

            if (s.Contains("fever"))
            {
                return "In Nigeria, common causes of fever include Malaria, Typhoid Fever, Viral Infection, Pneumonia, and Sepsis.";
            }

            if (s.Contains("headache"))
            {
                return "Headache may indicate Malaria, Typhoid Fever, Migraine, Hypertension, or Dehydration.";
            }

            if (s.Contains("vomiting"))
            {
                return "Vomiting may indicate Gastroenteritis, Food Poisoning, Typhoid Fever, Malaria, or Pregnancy-related nausea.";
            }

            if (s.Contains("diarrhea"))
            {
                return "Diarrhea may be caused by Cholera, Gastroenteritis, Foodborne infection, or Dysentery.";
            }

            if (s.Contains("rash"))
            {
                return "Skin rash may indicate Measles, Chickenpox, Allergic Reaction, Viral Infection, or Drug Reaction.";
            }

            if (s.Contains("cough"))
            {
                return "Cough may indicate Upper Respiratory Infection, Pneumonia, Tuberculosis, or COVID-like viral illness.";
            }

            if (s.Contains("abdominal pain"))
            {
                return "Abdominal pain may indicate Appendicitis, Typhoid Fever, Gastroenteritis, Peptic Ulcer Disease, or Ectopic Pregnancy.";
            }

            if (s.Contains("chest pain"))
            {
                return "Chest pain may indicate Pneumonia, Pulmonary Embolism, Cardiac disease, or severe infection.";
            }

            if (s.Contains("breathing"))
            {
                return "Breathing difficulty may indicate Asthma attack, Pneumonia, Severe infection, or Respiratory distress.";
            }

            if (s.Contains("seizure"))
            {
                return "Seizures may indicate severe malaria, epilepsy, meningitis, or head trauma.";
            }

            if (s.Contains("bleeding"))
            {
                return "Severe bleeding may indicate trauma, miscarriage, postpartum hemorrhage, or internal injury.";
            }

            return "No additional clinical knowledge available.";
        }
    }
}