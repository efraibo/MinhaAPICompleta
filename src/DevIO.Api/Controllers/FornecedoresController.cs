using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;

using DevIO.Api.Dtos;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    public class FornecedoresController : MainController
    {

        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IFornecedorService _fornecedorService;
        private readonly IMapper _mapper;

        public FornecedoresController(IFornecedorRepository fornecedorRepository,
                                      IMapper mapper,
                                      IFornecedorService fornecedorService,
                                      INotificador notificador,
                                      IEnderecoRepository enderecoRepository) : base(notificador)
        {
            _fornecedorRepository = fornecedorRepository;
            _mapper = mapper;
            _fornecedorService = fornecedorService;
            _enderecoRepository = enderecoRepository;
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<FornecedorDto>> ObterTodos()
        {
            IEnumerable<FornecedorDto> fornecedores = _mapper.Map<IEnumerable<FornecedorDto>>(await _fornecedorRepository.ObterTodos());

            return fornecedores;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> ObterPorId(Guid id)
        {
            var fornecedores = await ObterFornecdorProdutosEndereco(id);

            if (fornecedores == null) return NotFound();

            return fornecedores;
        }

        [HttpPost]
        public async Task<ActionResult<FornecedorDto>> Adicionar(FornecedorDto fornecedorDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.Adicionar(_mapper.Map<Fornecedor>(fornecedorDto));

            return CustomResponse(fornecedorDto);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> Atualizar(Guid id, FornecedorDto fornecedorDto)
        {
            if (id != fornecedorDto.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query");

                return CustomResponse(fornecedorDto);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);


            await _fornecedorService.Atualizar(_mapper.Map<Fornecedor>(fornecedorDto));


            return Ok(CustomResponse(fornecedorDto));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> Excluir(Guid id)
        {
            var fornecedorDto = await ObterFornecdorEndereco(id);

            if (fornecedorDto == null) return NotFound();

            await _fornecedorService.Remover(id);

            return CustomResponse(fornecedorDto);

        }


        [HttpGet("obter-endereco/{id:guid}")]
        public async Task<EnderecoDto> ObterEnderecoPorId(Guid id)
        {
            return _mapper.Map<EnderecoDto>(await _enderecoRepository.ObterPorId(id));
        }


        [HttpPut("atualizar-endereco/{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> AtualizarEndereco(Guid id, EnderecoDto enderecoDto)
        {
            if (id != enderecoDto.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query");

                return CustomResponse(enderecoDto);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);


            await _fornecedorService.AtualizarEndereco(_mapper.Map<Endereco>(enderecoDto));


            return Ok(CustomResponse(enderecoDto));
        }

        private async Task<FornecedorDto> ObterFornecdorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));
        }

        private async Task<FornecedorDto> ObterFornecdorEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorRepository.ObterFornecedorEndereco(id));
        }
    }
}
