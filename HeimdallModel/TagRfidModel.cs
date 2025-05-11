namespace HeimdallModel;
using System.Text.Json.Serialization;

public class TagRfidModel
{
public required int Id {get; set;}
public required int MotoId {get; set;}
public required string FaixaFrequencia {get; set;}
public required string Banda {get; set;}
public required string Aplicacao {get; set;}

[JsonIgnore]
public MotoModel? Moto { get; set; } 



}