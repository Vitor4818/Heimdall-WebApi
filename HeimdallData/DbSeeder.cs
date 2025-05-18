namespace HeimdallData;

using HeimdallModel;
public static class DbSeeder
{
    public static void SeedMotos(AppDbContext context)
    {
        if (context.Moto.Count() == 0)
        {
            var motos = new List<MotoModel>
            {
                new MotoModel { id = 1, tipoMoto = "Esportiva", placa = "ABC1234", numChassi = "9C2JC4110JR000001" },
                new MotoModel { id = 2, tipoMoto = "Custom", placa = "XYZ5678", numChassi = "9C2JC4110JR000002" },
                new MotoModel { id = 3, tipoMoto = "Naked", placa = "LMN9012", numChassi = "9C2JC4110JR000003" },
                new MotoModel { id = 4, tipoMoto = "Trail", placa = "DEF3456", numChassi = "9C2JC4110JR000004" },
                new MotoModel { id = 5, tipoMoto = "Scooter", placa = "GHI7890", numChassi = "9C2JC4110JR000005" }
            };

            context.Moto.AddRange(motos);
            context.SaveChanges();
        }
    }
}
