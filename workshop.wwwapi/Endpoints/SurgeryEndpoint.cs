﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using workshop.wwwapi.DTOs;
using workshop.wwwapi.Models;
using workshop.wwwapi.Repository;

namespace workshop.wwwapi.Endpoints
{
    public static class SurgeryEndpoint
    {
        public static void ConfigureSurgeryEndpoint(this WebApplication app)
        {
            var surgeryGroup = app.MapGroup("surgery");
            var patientGroup = app.MapGroup("patient");

            /*surgeryGroup.MapGet("/doctors", GetDoctors);
            surgeryGroup.MapGet("/appointmentsbydoctor/{id}", GetAppointmentsByDoctor);*/

            // Patient endpoints
            patientGroup.MapPost("/", CreatePatient);
            patientGroup.MapGet("/", GetPatients);
            patientGroup.MapGet("/{id}", GetPatientById);
        }

        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public static async Task<IResult> GetDoctors(IRepository<Doctor> repository)
        //{
        //    return TypedResults.Ok(await repository.GetDoctors());
        //}

        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public static async Task<IResult> GetAppointmentsByDoctor(IRepository repository, int id)
        //{
        //    return TypedResults.Ok(await repository.GetAppointmentsByDoctor(id));
        //}

        // Patient endpoints
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public static async Task<IResult> CreatePatient(IRepository<Patient> repository, PatientDto patientDto)
        {
            if (patientDto == null)
            {
                return TypedResults.BadRequest("Invalid data.");
            }

            var patient = new Patient
            {
                FirstName = patientDto.FirstName,
                LastName = patientDto.LastName
            };

            var createdPatient = await repository.Insert(patient);

            var createdPatientDto = new PatientDto
            {
                Id = createdPatient.Id,
                FirstName = createdPatient.FirstName,
                LastName = createdPatient.LastName
            };

            return TypedResults.Created($"/patient/{createdPatientDto.Id}", createdPatientDto);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetPatients(IRepository<Patient> repository)
        {
            var patients = await repository.Get();
            var patientDtos = patients.Select(p => new PatientDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName
            }).ToList();

            return TypedResults.Ok(patientDtos);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> GetPatientById(IRepository<Patient> repository, int id)
        {
            var patient = await repository.GetById(id);
            if (patient == null)
            {
                return TypedResults.NotFound(new { Message = "Patient not found" });
            }

            var patientDto = new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName
            };

            return TypedResults.Ok(patientDto);
        }
    }
}
