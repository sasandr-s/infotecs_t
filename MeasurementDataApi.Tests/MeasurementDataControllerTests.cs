using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MeasurementDataApi.Controllers;
using MeasurementDataApi.Models;
using MeasurementDataApi.Models.Dto;
using MeasurementDataApi.Repositories;
using MeasurementDataApi.Services;

namespace MeasurementDataApi.Tests;

public class MeasurementDataControllerTests
{
    private readonly Mock<IDataProcessingService> _dataServiceMock;
    private readonly Mock<IDataRepository> _repositoryMock;
    private readonly MeasurementDataController _controller;

    public MeasurementDataControllerTests()
    {
        _dataServiceMock = new Mock<IDataProcessingService>();
        _repositoryMock = new Mock<IDataRepository>();
        _controller = new MeasurementDataController(_dataServiceMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task UploadFile_ValidFile_ReturnsOk()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        
        _dataServiceMock.Setup(s => s.ProcessFileAsync(fileMock.Object)).ReturnsAsync(100);

        // Act
        var result = await _controller.UploadFile(fileMock.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UploadResultDto>(okResult.Value);
        Assert.Equal(100, dto.RecordsProcessed);
        Assert.Equal("test.csv", dto.FileName);
    }

    [Fact]
    public async Task GetResults_ReturnsOk()
    {
        // Arrange
        var filter = new ResultFilter();
        var results = new List<ResultRecord>
        {
            new ResultRecord { FileName = "test.csv", MaxValue = 10 }
        };
        _repositoryMock.Setup(r => r.GetResultsAsync(filter)).ReturnsAsync(results);

        // Act
        var result = await _controller.GetResults(filter);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result); 
        var dtoList = Assert.IsType<List<ResultDto>>(okResult.Value);
        Assert.Single(dtoList);
        Assert.Equal("test.csv", dtoList[0].FileName);
    }

    [Fact]
    public async Task GetLast10Values_ValidName_ReturnsOk()
    {
        // Arrange
        var fileName = "test.csv";
        var values = new List<ValueRecord>
        {
            new ValueRecord { FileName = fileName, Value = 10 }
        };
        _repositoryMock.Setup(r => r.GetLast10ValuesAsync(fileName)).ReturnsAsync(values);

        // Act
        var result = await _controller.GetLast10Values(fileName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtoList = Assert.IsType<List<ValueDto>>(okResult.Value);
        Assert.Single(dtoList);
        Assert.Equal(10, dtoList[0].Value);
    }

    [Fact]
    public async Task GetLast10Values_EmptyName_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetLast10Values("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = Assert.IsType<ErrorDto>(badRequestResult.Value);
        Assert.Contains("обязателен", error.Error);
    }
}
