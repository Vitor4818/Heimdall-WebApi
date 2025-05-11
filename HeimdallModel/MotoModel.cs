namespace HeimdallModel;
public class MotoModel
{
public required int id {get; set;}
public required string tipoMoto {get; set;}
public required string placa {get; set;}
public required string numChassi {get; set;}
public TagRfidModel? TagRfid { get; set; }




}