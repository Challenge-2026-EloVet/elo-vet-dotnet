using EloVet.Application.Interfaces;
using EloVet.Application.Services;
using EloVet.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace EloVet.UnitTests.Services;

public class ProntuarioServiceTests
{
    private readonly Mock<IProntuarioRepository> _mockRepository;
    private readonly Mock<ILogger<ProntuarioService>> _mockLogger;
    private readonly ProntuarioService _service;

    public ProntuarioServiceTests()
    {
        // Arrange inicial compartilhado por todos os testes
        _mockRepository = new Mock<IProntuarioRepository>();
        _mockLogger = new Mock<ILogger<ProntuarioService>>();

        _service = new ProntuarioService(
            _mockRepository.Object,
            _mockLogger.Object);
    }

    // ============================================================
    // SALVAR ASYNC
    // ============================================================

    [Fact]
    public async Task SalvarAsync_ProntuarioNaoExiste_SalvaProntuario()
    {
        // Arrange
        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = "pet-001"
            }
        };

        // Simula que o pet ainda não possui prontuário
        _mockRepository
            .Setup(r => r.FindByPetIdAsync(prontuario.Pet.Id))
            .ReturnsAsync((Prontuario?)null);

        // Act
        await _service.SalvarAsync(prontuario);

        // Assert
        _mockRepository.Verify(
            r => r.SalvarAsync(prontuario),
            Times.Once);
    }

    [Fact]
    public async Task SalvarAsync_ProntuarioJaExiste_LancaExcecao()
    {
        // Arrange
        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = "pet-001"
            }
        };

        var prontuarioExistente = new Prontuario
        {
            Pet = new Pet
            {
                Id = "pet-001"
            }
        };

        // Simula que o pet já possui prontuário
        _mockRepository
            .Setup(r => r.FindByPetIdAsync(prontuario.Pet.Id))
            .ReturnsAsync(prontuarioExistente);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SalvarAsync(prontuario));

        // Assert
        Assert.Equal(
            "O pet já possui um prontuário.",
            exception.Message);

        // Garante que o prontuário não foi salvo
        _mockRepository.Verify(
            r => r.SalvarAsync(It.IsAny<Prontuario>()),
            Times.Never);
    }

    // ============================================================
    // LISTAR ASYNC
    // ============================================================

    [Fact]
    public async Task ListarAsync_ProntuariosExistem_RetornaProntuarios()
    {
        // Arrange
        var prontuarios = new List<Prontuario>
        {
            new Prontuario
            {
                Id = "507f1f77bcf86cd799439011",
                Pet = new Pet
                {
                    Id = "pet-001"
                }
            },
            new Prontuario
            {
                Id = "507f1f77bcf86cd799439012",
                Pet = new Pet
                {
                    Id = "pet-002"
                }
            }
        };

        _mockRepository
            .Setup(r => r.ListarAsync())
            .ReturnsAsync(prontuarios);

        // Act
        var resultado = await _service.ListarAsync();

        // Assert
        Assert.Equal(2, resultado.Count());

        _mockRepository.Verify(
            r => r.ListarAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ListarAsync_NaoExistemProntuarios_RetornaListaVazia()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.ListarAsync())
            .ReturnsAsync(new List<Prontuario>());

        // Act
        var resultado = await _service.ListarAsync();

        // Assert
        Assert.Empty(resultado);

        _mockRepository.Verify(
            r => r.ListarAsync(),
            Times.Once);
    }

    // ============================================================
    // FIND BY ID ASYNC
    // ============================================================

    [Fact]
    public async Task FindByIdAsync_ProntuarioExiste_RetornaProntuario()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439011";

        var prontuario = new Prontuario
        {
            Id = id,
            Pet = new Pet
            {
                Id = "pet-001"
            }
        };

        _mockRepository
            .Setup(r => r.FindByIdAsync(id))
            .ReturnsAsync(prontuario);

        // Act
        var resultado = await _service.FindByIdAsync(id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado.Id);

        _mockRepository.Verify(
            r => r.FindByIdAsync(id),
            Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ProntuarioNaoExiste_RetornaNull()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439012";

        _mockRepository
            .Setup(r => r.FindByIdAsync(id))
            .ReturnsAsync((Prontuario?)null);

        // Act
        var resultado = await _service.FindByIdAsync(id);

        // Assert
        Assert.Null(resultado);

        _mockRepository.Verify(
            r => r.FindByIdAsync(id),
            Times.Once);
    }

    // ============================================================
    // FIND BY PET ID ASYNC
    // ============================================================

    [Fact]
    public async Task FindByPetIdAsync_ProntuarioExiste_RetornaProntuario()
    {
        // Arrange
        var petId = "pet-001";

        var prontuario = new Prontuario
        {
            Id = "507f1f77bcf86cd799439011",
            Pet = new Pet
            {
                Id = petId
            }
        };

        _mockRepository
            .Setup(r => r.FindByPetIdAsync(petId))
            .ReturnsAsync(prontuario);

        // Act
        var resultado = await _service.FindByPetIdAsync(petId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(petId, resultado.Pet.Id);

        _mockRepository.Verify(
            r => r.FindByPetIdAsync(petId),
            Times.Once);
    }

    [Fact]
    public async Task FindByPetIdAsync_ProntuarioNaoExiste_RetornaNull()
    {
        // Arrange
        var petId = "pet-999";

        _mockRepository
            .Setup(r => r.FindByPetIdAsync(petId))
            .ReturnsAsync((Prontuario?)null);

        // Act
        var resultado = await _service.FindByPetIdAsync(petId);

        // Assert
        Assert.Null(resultado);

        _mockRepository.Verify(
            r => r.FindByPetIdAsync(petId),
            Times.Once);
    }

    // ============================================================
    // EDITAR ASYNC
    // ============================================================

    [Fact]
    public async Task EditarAsync_ProntuarioExiste_AtualizaProntuario()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439011";

        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = "pet-001"
            }
        };

        _mockRepository
            .Setup(r => r.EditarAsync(id, prontuario))
            .ReturnsAsync(prontuario);

        // Act
        var resultado = await _service.EditarAsync(id, prontuario);

        // Assert
        Assert.NotNull(resultado);

        // O Service deve colocar o ID recebido na rota no objeto
        Assert.Equal(id, prontuario.Id);

        _mockRepository.Verify(
            r => r.EditarAsync(id, prontuario),
            Times.Once);
    }

    [Fact]
    public async Task EditarAsync_ProntuarioNaoExiste_RetornaNull()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439012";

        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = "pet-001"
            }
        };

        _mockRepository
            .Setup(r => r.EditarAsync(id, prontuario))
            .ReturnsAsync((Prontuario?)null);

        // Act
        var resultado = await _service.EditarAsync(id, prontuario);

        // Assert
        Assert.Null(resultado);

        // Mesmo que o ID não exista, o Service tentou realizar a atualização
        _mockRepository.Verify(
            r => r.EditarAsync(id, prontuario),
            Times.Once);
    }

    // ============================================================
    // EXCLUIR ASYNC
    // ============================================================

    [Fact]
    public async Task ExcluirAsync_ProntuarioExiste_ExcluiProntuario()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439011";

        _mockRepository
            .Setup(r => r.ExcluirAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ExcluirAsync(id);

        // Assert
        _mockRepository.Verify(
            r => r.ExcluirAsync(id),
            Times.Once);
    }
}