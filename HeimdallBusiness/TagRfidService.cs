using HeimdallModel;
using HeimdallData; 
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HeimdallBusiness
{
    public class TagRfidService
    {
        private readonly AppDbContext _context;

        public TagRfidService(AppDbContext context)
        {
            _context = context;
        }
    
       public IQueryable<TagRfidModel> ListarTags()
       {

            return _context.TagsRfid.AsQueryable();
       }

       public TagRfidModel? ObterPorId(int id)
       {
        return _context.TagsRfid.Find(id);
       }

       public TagRfidModel CadastrarTag(TagRfidModel tag)
       {
        _context.TagsRfid.Add(tag);
        _context.SaveChanges();
        return tag;
       }

        public bool AtualizarTag(TagRfidModel tag)
        {
            var existente = _context.TagsRfid.Find(tag.Id);
            if (existente == null) return false;

            existente.MotoId = tag.MotoId;
            existente.FaixaFrequencia = tag.FaixaFrequencia;
            existente.Banda = tag.Banda;
            existente.Aplicacao = tag.Aplicacao;
            _context.SaveChanges();
            return true;
        }

public bool RemoverTag(int id)
        {
            var tag = _context.TagsRfid.Find(id);
            if (tag == null) return false;
            _context.TagsRfid.Remove(tag);
            _context.SaveChanges();
            return true;
        }
    }

    
}

