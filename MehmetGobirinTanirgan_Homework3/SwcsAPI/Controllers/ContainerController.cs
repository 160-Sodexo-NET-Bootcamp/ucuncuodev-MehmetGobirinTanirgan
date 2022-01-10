using AutoMapper;
using Data.DataModels;
using Data.Uow.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwcsAPI.Dtos;
using SwcsAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwcsAPI.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class ContainerController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ContainerController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet]// Tüm container'ları getir
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var allContainers = await unitOfWork.Containers.GetAll().ToListAsync();

                if (allContainers.Count == 0)
                {
                    return NoContent();
                }

                //Automapper mapping ile Dto'ya dönüştürme
                var allContainerResponseDtos = mapper.Map<List<Container>, List<ContainerDefaultResponseDto>>(allContainers);
                return Ok(allContainerResponseDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]// Container ekleme
        public async Task<IActionResult> AddContainer([FromBody] ContainerCreateDto reqContainer)
        {
            // Automapper ile iki farklı türü birbirine çevirme
            var newContainer = mapper.Map<ContainerCreateDto, Container>(reqContainer);
            try
            {
                await unitOfWork.Containers.AddAsync(newContainer);
                await unitOfWork.SaveAsync();
                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpPut]// Container güncelleme
        public async Task<IActionResult> UpdateContainer([FromBody] ContainerUpdateDto reqContainer)
        {
            try
            {
                var existingContainer = await unitOfWork.Containers.GetByIdAsync(reqContainer.Id);
                if (existingContainer is null)
                {
                    return BadRequest("Container doesn't exist.");
                }

                //Automapper ile mevcut bir instance'ı güncelleme
                mapper.Map(reqContainer, existingContainer);

                unitOfWork.Containers.Update(existingContainer);
                await unitOfWork.SaveAsync();
                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]// Container silme
        public async Task<IActionResult> DeleteContainer([FromRoute] long id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid parameter.");
            }

            try
            {
                unitOfWork.Containers.Delete(id);
                await unitOfWork.SaveAsync();
                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{vehicleId}")]// Gelen vehicle id'ye ait container'ları getirme
        public async Task<IActionResult> GetContainersOfVehicle([FromRoute] long vehicleId)
        {
            if (vehicleId <= 0)
            {
                return BadRequest("Invalid parameter.");
            }

            try
            {
                var containersOfVehicle = await unitOfWork.Containers.
                    GetListByExpression(x => x.VehicleId == vehicleId).ToListAsync();

                if (containersOfVehicle.Count == 0)
                {
                    return NoContent();
                }

                //Automapper mapping ile Dto'ya dönüştürme
                var containerResponseDtosOfVehicle = mapper.Map<List<Container>, List<ContainerDefaultResponseDto>>(containersOfVehicle);
                return Ok(containerResponseDtosOfVehicle);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{vehicleId}/{n}")]// Gelen vehicle id'ye ait container'ları n adet eşit büyüklükte kümeye ayırma
        public async Task<IActionResult> GetClusteredContainersOfVehicle([FromRoute] long vehicleId, [FromRoute] int n)
        {
            if (vehicleId <= 0 || n <= 0)
            {
                return BadRequest("Invalid parameter.");
            }

            try
            {
                var containersOfVehicle = await unitOfWork.Containers.
                   GetListByExpression(x => x.VehicleId == vehicleId).ToListAsync();
                var containerCt = containersOfVehicle.Count;

                if (containerCt == 0)
                {
                    return NoContent();
                }

                if (n > containerCt / 2)
                {
                    return BadRequest("Number of clusters cannot be higher then " + containerCt / 2);
                }

                var clusteredContainers = containersOfVehicle.ToClusters(n);

                var clusteredContainerResponseDtos = mapper.Map<List<List<Container>>, List<List<ContainerDefaultResponseDto>>>(clusteredContainers);
                return Ok(clusteredContainerResponseDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
