using AutoMapper;
using DevIO.Api.Dtos;
using DevIO.Business.Models;

namespace DevIO.Api.Configuration
{

    public class AutomapperConfig : Profile
    {

        public AutomapperConfig()
        {
            CreateMap<Fornecedor, FornecedorDto>().ReverseMap();
            CreateMap<Endereco, EnderecoDto>().ReverseMap();
            CreateMap<Produto, ProdutoDto>().ReverseMap();
        }

    }
}
