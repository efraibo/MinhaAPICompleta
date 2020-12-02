using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using AutoMapper;

using DevIO.Api.Dtos;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers
{
    [Route("api/produtos")]
    public class ProdutoController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutoController(IProdutoRepository produtoRepository,
                                 IProdutoService produtoService,
                                 IMapper mapper,
                                 INotificador notificador) : base(notificador)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoDto>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoDto>>(await _produtoRepository.ObterProdutosFornecedores());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDto>> ObterPoId(Guid id)
        {
            var produtoDto = await ObterProduto(id);

            if (produtoDto == null) return NotFound();

            return produtoDto;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoDto>> Adicionar(ProdutoDto produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = $"{Guid.NewGuid()}_{produtoDto.Imagem}";

            if (!UploadArquivo(produtoDto.ImagemUpload, imagemNome))
            {
                return CustomResponse(produtoDto);
            }

            produtoDto.Imagem = imagemNome;

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutoDto>> AdicionarAlternativo(ProdutoImagemDto produtoImagemDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_";
            if (!await UploadArquivoAlternativo(produtoImagemDto.ImagemUpload, imgPrefixo))
            {
                return CustomResponse(ModelState);
            }

            produtoImagemDto.Imagem = imgPrefixo + produtoImagemDto.ImagemUpload.FileName;

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoImagemDto));

            return CustomResponse(produtoImagemDto);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoDto>> Excluir(Guid id)
        {
            var produtoDto = await ObterProduto(id);

            if (produtoDto == null) return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse(produtoDto);

        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, ProdutoDto produtoDto)
        {
            if (id != produtoDto.Id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return NotFound();
            }

            var produtoAtualizado = await ObterProduto(id);
            produtoDto.Imagem = produtoAtualizado.Imagem;
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (produtoDto.ImagemUpload != null)
            {
                var imagemNome = Guid.NewGuid() + "_" + produtoDto.Imagem;
                if (!UploadArquivo(produtoDto.ImagemUpload, imagemNome))
                {
                    return CustomResponse(ModelState);
                }

                produtoAtualizado.Imagem = imagemNome;
            }

            produtoAtualizado.Nome = produtoDto.Nome;
            produtoAtualizado.Descricao = produtoDto.Descricao;
            produtoAtualizado.Valor = produtoDto.Valor;
            produtoAtualizado.Ativo = produtoDto.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizado));

            return CustomResponse(produtoDto);

        }


        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Foneça uma imagem para este produto!");
                return false;
            }

            var imageDataByteArray = Convert.FromBase64String(arquivo);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;

        }


        private async Task<ProdutoDto> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoDto>(await _produtoRepository.ObterProdutoFornecedor(id));
        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }



    }

}
