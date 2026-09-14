using EloVet.Application.Interfaces;
using EloVet.Controllers.ProntuarioController;
using EloVet.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EloVet.UnitTests.Controllers;

public class ProntuarioControllerTests
{
    private readonly Mock<IProntuarioService> _mockService;
    private readonly Mock<ILogger<ProntuarioController>> _mockLogger;
    private readonly ProntuarioController _controller;

    public ProntuarioControllerTests()
    {
        // Setup inicial
        _mockService = new Mock<IProntuarioService>();
        _mockLogger = new Mock<ILogger<ProntuarioController>>();

        _controller = new ProntuarioController(
            _mockService.Object,
            _mockLogger.Object);
    }

    // ============================================================
    // SALVAR ASYNC
    // ============================================================

    [Fact]
    public async Task SalvarAsync_ProntuarioValido_RetornaCreated()
    {
        // Caso de uso: cadastrar um prontuário válido.

        // Arrange
        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = "pet-001"
            }
        };

        _mockService
            .Setup(s => s.SalvarAsync(prontuario))
            .ReturnsAsync(prontuario);

        // Act
        var resultado = await _controller.SalvarAsync(prontuario);

        // Assert
        Assert.IsType<CreatedResult>(resultado);

        _mockService.Verify(
            s => s.SalvarAsync(prontuario),
            Times.Once);
    }

    [Fact]
    public async Task SalvarAsync_ProntuarioJaExiste_RetornaConflict()
    {
        // Arrange
        var prontuario = new Prontuario
        {
            Pet = new Pet
            {
                Id = "pet-001"
            }
        };

        _mockService
            .Setup(s => s.SalvarAsync(prontuario))
            .ThrowsAsync(
                new InvalidOperationException(
                    "O pet já possui um prontuário."));

        // Act
        var resultado = await _controller.SalvarAsync(prontuario);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(resultado);

        Assert.Equal(
            "O pet já possui um prontuário.",
            conflictResult.Value);
    }

    // ============================================================
    // LISTAR ASYNC
    // ============================================================

    [Fact]
    public async Task ListarAsync_ProntuariosExistem_RetornaOk()
    {
        // Arrange
        var prontuarios = new List<Prontuario>
        {
            new Prontuario
            {
                Id = "prontuario-001",
                Pet = new Pet
                {
                    Id = "pet-001"
                }
            },
            new Prontuario
            {
                Id = "prontuario-002",
                Pet = new Pet
                {
                    Id = "pet-002"
                }
            }
        };

        _mockService
            .Setup(s => s.ListarAsync())
            .ReturnsAsync(prontuarios);

        // Act
        var resultado = await _controller.ListarAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);

        var prontuariosRetornados =
            Assert.IsAssignableFrom<IEnumerable<Prontuario>>(
                okResult.Value);

        Assert.Equal(2, prontuariosRetornados.Count());

        _mockService.Verify(
            s => s.ListarAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ListarAsync_NaoExistemProntuarios_RetornaOk()
    {
        // Arrange
        _mockService
            .Setup(s => s.ListarAsync())
            .ReturnsAsync(new List<Prontuario>());

        // Act
        var resultado = await _controller.ListarAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);

        var prontuariosRetornados =
            Assert.IsAssignableFrom<IEnumerable<Prontuario>>(
                okResult.Value);

        Assert.Empty(prontuariosRetornados);
    }

    // ============================================================
    // FIND BY ID ASYNC
    // ============================================================

    [Fact]
    public async Task FindByIdAsync_ProntuarioExiste_RetornaOk()
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

        _mockService
            .Setup(s => s.FindByIdAsync(id))
            .ReturnsAsync(prontuario);

        // Act
        var resultado = await _controller.FindByIdAsync(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);

        var prontuarioRetornado =
            Assert.IsType<Prontuario>(okResult.Value);

        Assert.Equal(id, prontuarioRetornado.Id);

        _mockService.Verify(
            s => s.FindByIdAsync(id),
            Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439012";

        _mockService
            .Setup(s => s.FindByIdAsync(id))
            .ReturnsAsync((Prontuario?)null);

        // Act
        var resultado = await _controller.FindByIdAsync(id);

        // Assert
        Assert.IsType<NotFoundResult>(resultado);

        _mockService.Verify(
            s => s.FindByIdAsync(id),
            Times.Once);
    }

    // ============================================================
    // FIND BY PET ID ASYNC
    // ============================================================

    [Fact]
    public async Task FindByPetIdAsync_ProntuarioExiste_RetornaOk()
    {
        // Arrange
        var petId = "pet-001";

        var prontuario = new Prontuario
        {
            Id = "prontuario-001",
            Pet = new Pet
            {
                Id = petId
            }
        };

        _mockService
            .Setup(s => s.FindByPetIdAsync(petId))
            .ReturnsAsync(prontuario);

        // Act
        var resultado = await _controller.FindByPetIdAsync(petId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);

        var prontuarioRetornado =
            Assert.IsType<Prontuario>(okResult.Value);

        Assert.Equal(petId, prontuarioRetornado.Pet.Id);

        _mockService.Verify(
            s => s.FindByPetIdAsync(petId),
            Times.Once);
    }

    [Fact]
    public async Task FindByPetIdAsync_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Arrange
        var petId = "pet-999";

        _mockService
            .Setup(s => s.FindByPetIdAsync(petId))
            .ReturnsAsync((Prontuario?)null);

        // Act
        var resultado = await _controller.FindByPetIdAsync(petId);

        // Assert
        Assert.IsType<NotFoundResult>(resultado);

        _mockService.Verify(
            s => s.FindByPetIdAsync(petId),
            Times.Once);
    }

    // ============================================================
    // EDITAR ASYNC
    // ============================================================

    [Fact]
    public async Task EditarAsync_ProntuarioValido_RetornaOk()
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

        _mockService
            .Setup(s => s.EditarAsync(id, prontuario))
            .ReturnsAsync(prontuario);

        // Act
        var resultado =
            await _controller.EditarAsync(id, prontuario);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);

        var prontuarioRetornado =
            Assert.IsType<Prontuario>(okResult.Value);

        Assert.Equal(id, prontuarioRetornado.Id);

        _mockService.Verify(
            s => s.EditarAsync(id, prontuario),
            Times.Once);
    }

    [Fact]
    public async Task EditarAsync_IdDaRotaDiferenteDoBody_RetornaBadRequest()
    {
        // Arrange
        var idDaRota = "507f1f77bcf86cd799439011";

        var prontuario = new Prontuario
        {
            Id = "507f1f77bcf86cd799439012",
            Pet = new Pet
            {
                Id = "pet-001"
            }
        };

        // Act
        var resultado =
            await _controller.EditarAsync(idDaRota, prontuario);

        // Assert
        var badRequestResult =
            Assert.IsType<BadRequestObjectResult>(resultado);

        Assert.Equal(
            "O id não corresponde ao id do prontuário no body.",
            badRequestResult.Value);

        _mockService.Verify(
            s => s.EditarAsync(
                It.IsAny<string>(),
                It.IsAny<Prontuario>()),
            Times.Never);
    }

    [Fact]
    public async Task EditarAsync_ProntuarioNaoExiste_RetornaNotFound()
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

        _mockService
            .Setup(s => s.EditarAsync(id, prontuario))
            .ReturnsAsync((Prontuario?)null);

        // Act
        var resultado =
            await _controller.EditarAsync(id, prontuario);

        // Assert
        Assert.IsType<NotFoundResult>(resultado);

        _mockService.Verify(
            s => s.EditarAsync(id, prontuario),
            Times.Once);
    }

    // ============================================================
    // EXCLUIR ASYNC
    // ============================================================

    [Fact]
    public async Task ExcluirAsync_ProntuarioNaoExiste_RetornaNotFound()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439012";

        _mockService
            .Setup(s => s.FindByIdAsync(id))
            .ReturnsAsync((Prontuario?)null);

        // Act
        var resultado = await _controller.ExcluirAsync(id);

        // Assert
        Assert.IsType<NotFoundResult>(resultado);

        _mockService.Verify(
            s => s.ExcluirAsync(id),
            Times.Never);
    }

    [Fact]
    public async Task ExcluirAsync_ProntuarioExiste_RetornaNoContent()
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

        _mockService
            .Setup(s => s.FindByIdAsync(id))
            .ReturnsAsync(prontuario);

        _mockService
            .Setup(s => s.ExcluirAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.ExcluirAsync(id);

        // Assert
        Assert.IsType<NoContentResult>(resultado);

        _mockService.Verify(
            s => s.FindByIdAsync(id),
            Times.Once);

        _mockService.Verify(
            s => s.ExcluirAsync(id),
            Times.Once);
    }
}