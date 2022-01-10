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
    [Route("api/[controller]")]
    [ApiController]
    public class OptimizationController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public OptimizationController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet("{vehicleId}/{n}")]// K Means algoritmasına göre n adet kümeye ayırma
        public async Task<IActionResult> GetOptimizedClusters([FromRoute] long vehicleId, [FromRoute] int n)
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

                if (n > containerCt / 2) // Bu sınırı ben koydum. Kayıt sayısının yarısını geçmesini istemedim.
                {
                    return BadRequest("Number of clusters cannot be higher then " + containerCt / 2);
                }

                if (n == 1)
                {
                    return Ok(containersOfVehicle);
                }

                var clusteredContainers = containersOfVehicle.ToKMeansClusters(n);
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
