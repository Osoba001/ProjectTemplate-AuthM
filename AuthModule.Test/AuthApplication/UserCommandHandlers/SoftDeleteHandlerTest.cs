﻿using Auth.Application.Commands;
using Auth.Application.MediatR;
using Auth.Application.Models;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Utilities.Responses;

namespace AuthModule.Test.AuthApplication.UserHandlers
{
    public class SoftDeleteHandlerTest
    {

        private readonly Mock<IServiceWrapper> mockService;
        private readonly SoftDeleteCommand command = new() { Id = Guid.NewGuid() };
        private readonly SoftDeleteHandler handler = new();
        public SoftDeleteHandlerTest()
        {
            mockService = new Mock<IServiceWrapper>();

        }

        [Fact]
        public async void HandlerAsync_ReturnsError_WhenUserNotFound()
        {
            //Arrange
            mockService.Setup(s => s.UserRepo.FindById(It.IsAny<Guid>()))
                .ReturnsAsync(It.IsAny<UserModel>());

            //Act
            var result = await handler.HandleAsync(command, mockService.Object);

            //Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ReasonPhrase.Should().Contain(UserNotFound);
            mockService.Verify(s => s.UserRepo.FindById(It.IsAny<Guid>()), Times.Once());
        }

        [Fact]
        public async void HandlerAsync_ReturnsError_WhenDeleteIsNotSuccessful()
        {
            //Arrange
            mockService.Setup(s => s.UserRepo.FindById(It.IsAny<Guid>()))
                .ReturnsAsync(Users[0]);

            var failedActionResult = new KOActionResult();
            failedActionResult.AddError("Error");
            mockService.Setup(s => s.UserRepo.Update(It.IsAny<UserModel>()))
                .ReturnsAsync(failedActionResult);

            //Act
            var result = await handler.HandleAsync(command, mockService.Object);

            //Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            mockService.Verify(s => s.UserRepo.FindById(It.IsAny<Guid>()), Times.Once());
            mockService.Verify(s => s.UserRepo.Update(It.IsAny<UserModel>()), Times.Once());
        }

        [Fact]
        public async void HandlerAsync_ReturnsSuccess_WhenDeleteIsSuccessful()
        {
            //Arrange
            mockService.Setup(s => s.UserRepo.FindById(It.IsAny<Guid>()))
                .ReturnsAsync(Users[0]);

            mockService.Setup(s => s.UserRepo.Update(It.IsAny<UserModel>()))
                .ReturnsAsync(new KOActionResult());

            //Act
            var result = await handler.HandleAsync(command, mockService.Object);

            //Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            mockService.Verify(s => s.UserRepo.FindById(It.IsAny<Guid>()), Times.Once());
            mockService.Verify(s => s.UserRepo.Update(It.IsAny<UserModel>()), Times.Once());
        }
    }
}
