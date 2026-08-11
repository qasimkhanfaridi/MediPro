using MediPro.Api.Entities;

namespace MediPro.Api.Data;

/// <summary>Deterministic demo SKUs for QA (tablets, caps, syrups, strengths, manufacturers).</summary>
public static class DemoCatalogSeed
{
    public const string SkuPrefix = "TEST-MED-";

    /// <summary>Placeholder images: one distinct image per SKU (Lorem Picsum, deterministic seed).</summary>
    public static string ImageUrlForSku(string skuCode) =>
        $"https://picsum.photos/seed/{Uri.EscapeDataString(skuCode)}/400/280";

    public static IReadOnlyList<Product> BuildProducts(Guid tenantId, DateTime nowUtc)
    {
        var rows = GetRows();
        var list = new List<Product>(rows.Count);
        foreach (var r in rows)
        {
            var sku = $"{SkuPrefix}{r.Index:D3}";
            list.Add(new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SkuCode = sku,
                Name = r.Name,
                Pack = r.Pack,
                Manufacturer = r.Manufacturer,
                SaltComposition = r.Salt,
                Category = r.Category,
                TradePrice = r.Trade,
                Mrp = r.Mrp,
                IsActive = true,
                StockQuantity = r.Stock,
                ImageUrl = ImageUrlForSku(sku),
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc,
            });
        }

        return list;
    }

    private sealed record Row(
        int Index,
        string Name,
        string Pack,
        string Manufacturer,
        string Salt,
        string Category,
        decimal Trade,
        decimal Mrp,
        int Stock);

    private static IReadOnlyList<Row> GetRows()
    {
        var mfgs = new[]
        {
            "Getz Pharma", "Searle Pakistan", "Abbott Laboratories", "GlaxoSmithKline Pakistan",
            "Sanofi-Aventis", "Novartis Pharma", "Martin Dow", "Highnoon Laboratories",
            "Ferozsons Laboratories", "Hilton Pharma", "AGP Limited", "Standpharm Pakistan",
            "Genetics Pharmaceuticals", "Brookes Pharma", "Next Health Pakistan", "Raazee Therapeutics",
        };

        // (display name suffix, pack, salt line, category code, trade, mrp, stock)
        var specs = new (string Name, string Pack, string Salt, string Cat, decimal T, decimal M, int S)[]
        {
            ("Paracetamol 500mg Tablets", "10's", "Paracetamol 500 mg", "Tablet", 120, 150, 420),
            ("Paracetamol 650mg Tablets", "10's", "Paracetamol 650 mg", "Tablet", 145, 180, 310),
            ("Ibuprofen 400mg Tablets", "20's", "Ibuprofen 400 mg", "Tablet", 210, 260, 280),
            ("Diclofenac Sodium 50mg Tablets", "20's", "Diclofenac sodium 50 mg", "Tablet", 95, 120, 500),
            ("Amoxicillin 250mg Capsules", "21's", "Amoxicillin (as trihydrate) 250 mg", "Capsule", 380, 450, 220),
            ("Amoxicillin 500mg Capsules", "15's", "Amoxicillin (as trihydrate) 500 mg", "Capsule", 520, 620, 190),
            ("Azithromycin 250mg Capsules", "6's", "Azithromycin 250 mg", "Capsule", 640, 760, 160),
            ("Omeprazole 20mg Capsules", "14's", "Omeprazole 20 mg", "Capsule", 290, 350, 340),
            ("Omeprazole 40mg Capsules", "14's", "Omeprazole 40 mg", "Capsule", 410, 490, 260),
            ("Cephalexin 250mg Capsules", "20's", "Cephalexin 250 mg", "Capsule", 340, 400, 210),
            ("Cephalexin 500mg Capsules", "20's", "Cephalexin 500 mg", "Capsule", 560, 670, 175),
            ("Metformin 500mg Tablets", "30's", "Metformin HCl 500 mg", "Tablet", 180, 220, 600),
            ("Metformin 850mg Tablets", "30's", "Metformin HCl 850 mg", "Tablet", 220, 270, 440),
            ("Glimepiride 2mg Tablets", "30's", "Glimepiride 2 mg", "Tablet", 260, 310, 330),
            ("Atorvastatin 10mg Tablets", "30's", "Atorvastatin calcium 10 mg", "Tablet", 310, 370, 390),
            ("Atorvastatin 20mg Tablets", "30's", "Atorvastatin calcium 20 mg", "Tablet", 420, 500, 360),
            ("Amlodipine 5mg Tablets", "30's", "Amlodipine besylate 5 mg", "Tablet", 140, 170, 520),
            ("Amlodipine 10mg Tablets", "30's", "Amlodipine besylate 10 mg", "Tablet", 190, 230, 410),
            ("Losartan 50mg Tablets", "30's", "Losartan potassium 50 mg", "Tablet", 240, 290, 370),
            ("Hydrochlorothiazide 25mg Tablets", "30's", "Hydrochlorothiazide 25 mg", "Tablet", 85, 105, 480),
            ("Cetirizine 10mg Tablets", "30's", "Cetirizine HCl 10 mg", "Tablet", 110, 135, 550),
            ("Loratadine 10mg Tablets", "20's", "Loratadine 10 mg", "Tablet", 125, 150, 430),
            ("Montelukast 10mg Tablets", "14's", "Montelukast sodium 10 mg", "Tablet", 480, 570, 200),
            ("Levofloxacin 500mg Tablets", "10's", "Levofloxacin 500 mg", "Tablet", 720, 860, 140),
            ("Ciprofloxacin 250mg Tablets", "10's", "Ciprofloxacin HCl 250 mg", "Tablet", 260, 310, 250),
            ("Ciprofloxacin 500mg Tablets", "10's", "Ciprofloxacin HCl 500 mg", "Tablet", 340, 410, 230),
            ("Metronidazole 400mg Tablets", "21's", "Metronidazole 400 mg", "Tablet", 155, 185, 310),
            ("Fluconazole 150mg Capsule", "1's", "Fluconazole 150 mg", "Capsule", 95, 115, 800),
            ("Fluconazole 200mg Capsules", "7's", "Fluconazole 200 mg", "Capsule", 380, 450, 190),
            ("Vitamin B Complex Capsules", "30's", "Thiamine, Riboflavin, Pyridoxine, Nicotinamide, Cyanocobalamin", "Capsule", 160, 195, 470),
            ("Calcium + Vitamin D3 Tablets", "30's", "Calcium carbonate 500 mg, Cholecalciferol 200 IU", "Tablet", 210, 250, 390),
            ("Iron + Folic Acid Tablets", "30's", "Ferrous sulfate 200 mg, Folic acid 0.5 mg", "Tablet", 130, 155, 510),
            ("ORS Sachets Lemon", "20 sachets", "Glucose, Sodium chloride, Potassium citrate, Sodium citrate", "Sachet", 240, 290, 620),
            ("Paracetamol 120mg/5ml Syrup", "60 ml", "Paracetamol 120 mg per 5 ml", "Syrup", 85, 105, 440),
            ("Paracetamol 250mg/5ml Suspension", "100 ml", "Paracetamol 250 mg per 5 ml", "Suspension", 110, 135, 360),
            ("Amoxicillin 125mg/5ml Suspension", "100 ml", "Amoxicillin 125 mg per 5 ml", "Suspension", 195, 235, 290),
            ("Amoxicillin 250mg/5ml Suspension", "100 ml", "Amoxicillin 250 mg per 5 ml", "Suspension", 240, 290, 270),
            ("Azithromycin 200mg/5ml Suspension", "15 ml", "Azithromycin 200 mg per 5 ml", "Suspension", 320, 380, 180),
            ("Cefixime 100mg/5ml Suspension", "30 ml", "Cefixime 100 mg per 5 ml", "Suspension", 410, 490, 165),
            ("Ibuprofen 100mg/5ml Suspension", "100 ml", "Ibuprofen 100 mg per 5 ml", "Suspension", 125, 150, 330),
            ("Lactulose 3.35g/5ml Syrup", "120 ml", "Lactulose 3.35 g per 5 ml", "Syrup", 175, 210, 220),
            ("Salbutamol 2mg/5ml Syrup", "150 ml", "Salbutamol sulfate 2 mg per 5 ml", "Syrup", 95, 115, 400),
            ("Ambroxol 15mg/5ml Syrup", "100 ml", "Ambroxol HCl 15 mg per 5 ml", "Syrup", 105, 125, 350),
            ("Multivitamin Syrup", "120 ml", "Vitamins A, D, E, B-complex, Vitamin C", "Syrup", 155, 185, 300),
            ("Insulin NPH 100IU/ml Injection", "10 ml vial", "Isophane insulin 100 IU/ml", "Injection", 890, 1050, 95),
            ("Insulin Regular 100IU/ml Injection", "10 ml vial", "Soluble insulin 100 IU/ml", "Injection", 870, 1030, 88),
            ("Ceftriaxone 1g IM/IV Injection", "1 vial + diluent", "Ceftriaxone sodium 1 g", "Injection", 240, 290, 210),
            ("Ceftriaxone 250mg IM Injection", "1 vial", "Ceftriaxone sodium 250 mg", "Injection", 120, 145, 260),
            ("Diclofenac Sodium 75mg/3ml Ampoule", "5 ampoules", "Diclofenac sodium 75 mg per 3 ml", "Injection", 180, 215, 170),
            ("Ranitidine 50mg/2ml Injection", "5 ampoules", "Ranitidine HCl 50 mg per 2 ml", "Injection", 95, 115, 140),
            ("Hydrocortisone 100mg Injection", "1 vial", "Hydrocortisone sodium succinate 100 mg", "Injection", 210, 250, 120),
            ("Mometasone 0.1% Cream", "15 g", "Mometasone furoate 0.1% w/w", "Cream", 195, 235, 240),
            ("Betamethasone 0.05% Cream", "15 g", "Betamethasone valerate 0.05% w/w", "Cream", 175, 210, 220),
            ("Fusidic Acid 2% Cream", "15 g", "Fusidic acid 2% w/w", "Cream", 220, 265, 200),
            ("Clotrimazole 1% Cream", "20 g", "Clotrimazole 1% w/w", "Cream", 125, 150, 310),
            ("Permethrin 5% Cream", "30 g", "Permethrin 5% w/w", "Cream", 260, 310, 150),
            ("Mupirocin 2% Ointment", "15 g", "Mupirocin 2% w/w", "Ointment", 310, 370, 130),
            ("Zinc Oxide 20% Ointment", "30 g", "Zinc oxide 20% w/w", "Ointment", 95, 115, 280),
            ("Artificial Tears 0.5% Eye Drops", "10 ml", "Carboxymethylcellulose sodium 0.5%", "Drops", 145, 175, 360),
            ("Moxifloxacin 0.5% Eye Drops", "5 ml", "Moxifloxacin HCl 0.5% w/v", "Drops", 380, 450, 155),
            ("Timolol 0.5% Eye Drops", "5 ml", "Timolol maleate 0.5% w/v", "Drops", 210, 250, 175),
            ("Clotrimazole 1% Ear Drops", "15 ml", "Clotrimazole 1% w/v", "Drops", 135, 160, 205),
            ("ORS Ready-to-Drink 200ml", "6 bottles", "Oral rehydration salts (WHO)", "Oral solution", 180, 215, 400),
            ("Zinc Sulphate 20mg Dispersible Tablets", "10's", "Zinc sulfate monohydrate 20 mg", "Tablet", 55, 70, 640),
            ("Esomeprazole 40mg Capsules", "14's", "Esomeprazole magnesium 40 mg", "Capsule", 520, 620, 210),
            ("Pantoprazole 40mg Tablets", "14's", "Pantoprazole sodium 40 mg", "Tablet", 340, 405, 275),
            ("Rabeprazole 20mg Tablets", "14's", "Rabeprazole sodium 20 mg", "Tablet", 290, 345, 240),
            ("Domperidone 10mg Tablets", "30's", "Domperidone maleate 10 mg", "Tablet", 125, 150, 410),
            ("Ondansetron 4mg Tablets", "10's", "Ondansetron HCl 4 mg", "Tablet", 195, 235, 180),
            ("Tramadol 50mg Capsules", "10's", "Tramadol HCl 50 mg", "Capsule", 180, 215, 155),
            ("Tramadol 100mg SR Tablets", "10's", "Tramadol HCl 100 mg sustained release", "Tablet", 310, 370, 140),
            ("Gabapentin 300mg Capsules", "30's", "Gabapentin 300 mg", "Capsule", 480, 575, 165),
            ("Pregabalin 75mg Capsules", "14's", "Pregabalin 75 mg", "Capsule", 560, 670, 150),
            ("Sertraline 50mg Tablets", "30's", "Sertraline HCl 50 mg", "Tablet", 410, 490, 195),
            ("Escitalopram 10mg Tablets", "28's", "Escitalopram oxalate 10 mg", "Tablet", 390, 465, 205),
            ("Aspirin 75mg Gastro-resistant Tablets", "30's", "Acetylsalicylic acid 75 mg", "Tablet", 85, 102, 520),
            ("Clopidogrel 75mg Tablets", "28's", "Clopidogrel bisulfate 75 mg", "Tablet", 420, 500, 230),
            ("Warfarin 5mg Tablets", "30's", "Warfarin sodium 5 mg", "Tablet", 95, 115, 170),
            ("Folic Acid 5mg Tablets", "100's", "Folic acid 5 mg", "Tablet", 45, 55, 890),
            ("Vitamin D3 200000 IU Oral Ampoule", "1 ampoule", "Cholecalciferol 200000 IU", "Oral solution", 120, 145, 320),
            ("Calcium + Magnesium + Zinc Syrup", "120 ml", "Calcium gluconate, Magnesium, Zinc", "Syrup", 135, 162, 300),
            ("Dexamethasone 0.5mg Tablets", "100's", "Dexamethasone 0.5 mg", "Tablet", 75, 90, 440),
            ("Prednisolone 5mg Tablets", "30's", "Prednisolone 5 mg", "Tablet", 110, 132, 360),
            ("Salbutamol Inhaler 100mcg", "200 doses", "Salbutamol 100 mcg per actuation", "Inhaler", 380, 455, 125),
            ("Budesonide 0.5mg/2ml Nebuliser", "20 unit-dose", "Budesonide 0.5 mg per 2 ml", "Nebuliser solution", 520, 620, 95),
            ("Insulin Aspart 100IU/ml Penfill", "5×3 ml", "Insulin aspart 100 IU/ml", "Injection", 1250, 1480, 72),
            ("Enoxaparin 40mg/0.4ml Prefilled Syringe", "10 syringes", "Enoxaparin sodium 40 mg", "Injection", 2100, 2480, 60),
        };

        var list = new List<Row>(specs.Length);
        for (var i = 0; i < specs.Length; i++)
        {
            var s = specs[i];
            var idx = i + 1;
            list.Add(new Row(
                idx,
                s.Name,
                s.Pack,
                mfgs[i % mfgs.Length],
                s.Salt,
                s.Cat,
                s.T,
                s.M,
                s.S));
        }

        return list;
    }
}
