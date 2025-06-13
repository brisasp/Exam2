using System.Text.Json.Serialization;


namespace LoginRegister.Models
{
    public class DicatadorDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("usuario")]
        public int Usuario { get; set; }

        [JsonPropertyName("resultado")]
        public int Resultado { get; set; }

        [JsonPropertyName("fechaInicio")]
        public DateTime FechaInicio { get; set; }

        [JsonPropertyName("fechaFin")]
        public DateTime FechaFin { get; set; }

    }
}

  
  





