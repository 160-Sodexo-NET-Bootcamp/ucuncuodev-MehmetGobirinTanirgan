using AutoMapper;
using Data.DataModels;
using Data.Uow.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwcsAPI.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwcsAPI.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public VehicleController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet]// Tüm vehicle'ları getir
        public async Task<IActionResult> GetAllVehicles()
        {
            //Direkt olarak try-catch kullanarak sorun olması durumunda mesajı gönderdim.
            try
            {
                var allVehicles = await unitOfWork.Vehicles.GetAll().ToListAsync();
                if (allVehicles.Count == 0)
                {
                    return NoContent();
                }

                var allVehicleResponseDtos = mapper.Map<List<Vehicle>, List<VehicleDefaultResponseDto>>(allVehicles);
                return Ok(allVehicleResponseDtos);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpPost]// Vehicle ekleme
        public async Task<IActionResult> AddVehicle([FromBody] VehicleCreateDto reqVehicle)
        {
            // Automapper mapping
            var newVehicle = mapper.Map<VehicleCreateDto, Vehicle>(reqVehicle);

            try
            {
                await unitOfWork.Vehicles.AddAsync(newVehicle);
                await unitOfWork.SaveAsync();
                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpPut]// Vehicle güncelleme
        public async Task<IActionResult> UpdateVehicle([FromBody] VehicleUpdateDto reqVehicle)
        {
            try
            {
                var existingVehicle = await unitOfWork.Vehicles.GetByIdAsync(reqVehicle.Id);

                if (existingVehicle is null)
                {
                    return BadRequest("Vehicle does not exist.");
                }

                mapper.Map(reqVehicle, existingVehicle);
                unitOfWork.Vehicles.Update(existingVehicle);
                await unitOfWork.SaveAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")] // Vehicle'ı ve ona ait tüm container'ları silme
        public async Task<IActionResult> DeleteVehicle([FromRoute] long id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid parameter.");
            }

            try
            {
                unitOfWork.Vehicles.Delete(id);
                await unitOfWork.Containers.DeleteRangeByExpressionAsync(x => x.VehicleId == id);
                await unitOfWork.SaveAsync();
                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById([FromRoute] long id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid parameter.");
            }

            try
            {
                var vehicle = await unitOfWork.Vehicles.GetByIdAsync(id);

                if (vehicle is null)
                {
                    return NotFound();
                }

                var vehicleResponseDto = mapper.Map<Vehicle, VehicleDefaultResponseDto>(vehicle);
                return Ok(vehicleResponseDto);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }


    }
}
