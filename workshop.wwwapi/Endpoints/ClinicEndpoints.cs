using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using workshop.wwwapi.DTOs;
using workshop.wwwapi.Models;
using workshop.wwwapi.Repository;

namespace workshop.wwwapi.Endpoints
{
    public static class ClinicEndpoints
    {
        public static void ConfigureClinicEndpoints(this WebApplication app)
        {

            var appointmentGroup = app.MapGroup("appointment");
            var doctorGroup = app.MapGroup("doctor");
            var patientGroup = app.MapGroup("patient");

            // Appointment endpoints
            appointmentGroup.MapPost("/", CreateAppointment);
            appointmentGroup.MapGet("/", GetAppointments);
            appointmentGroup.MapGet("/{id}", GetAppointmentById);
            appointmentGroup.MapGet("/by-doctor/{doctorId}", GetAppointmentsByDoctorId);
            appointmentGroup.MapGet("/by-patient/{patientId}", GetAppointmentsByPatientId);

            // Doctor endpoints
            doctorGroup.MapPost("/", CreateDoctor);
            doctorGroup.MapGet("/", GetDoctors);
            doctorGroup.MapGet("/{id}", GetDoctorById);

            // Patient endpoints
            patientGroup.MapPost("/", CreatePatient);
            patientGroup.MapGet("/", GetPatients);
            patientGroup.MapGet("/{id}", GetPatientById);
        }

        // Appointment endpoints
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public static async Task<IResult> CreateAppointment(IRepository<Appointment> appointmentRepository, IRepository<Doctor> doctorRepository, IRepository<Patient> patientRepository, CreateAppointmentDTO createAppointmentDto)
        {
            if (createAppointmentDto == null)
            {
                return TypedResults.BadRequest("Invalid data.");
            }

            var doctor = await doctorRepository.GetById(createAppointmentDto.DoctorId);
            var patient = await patientRepository.GetById(createAppointmentDto.PatientId);

            if (doctor == null)
            {
                return TypedResults.BadRequest("Doctor not found.");
            }

            if (patient == null)
            {
                return TypedResults.BadRequest("Patient not found.");
            }

            var appointment = new Appointment
            {
                DoctorId = createAppointmentDto.DoctorId,
                PatientId = createAppointmentDto.PatientId,
                AppointmentDateTime = createAppointmentDto.AppointmentDateTime
            };

            var createdAppointment = await appointmentRepository.Insert(appointment);

            var createdAppointmentDto = new AppointmentDTO
            {
                Id = createdAppointment.Id,
                DoctorId = createdAppointment.DoctorId,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                PatientId = createdAppointment.PatientId,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                AppointmentDateTime = createdAppointment.AppointmentDateTime
            };

            return TypedResults.Created($"/appointment/{createdAppointmentDto.Id}", createdAppointmentDto);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetAppointments(IRepository<Appointment> appointmentRepository, IRepository<Doctor> doctorRepository, IRepository<Patient> patientRepository)
        {
            var appointments = await appointmentRepository.Get();

            var appointmentDtos = new List<AppointmentDTO>();

            foreach (var appointment in appointments)
            {
                var doctor = await doctorRepository.GetById(appointment.DoctorId);
                var patient = await patientRepository.GetById(appointment.PatientId);

                if (doctor != null && patient != null)
                {
                    appointmentDtos.Add(new AppointmentDTO
                    {
                        Id = appointment.Id,
                        DoctorId = appointment.DoctorId,
                        DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                        PatientId = appointment.PatientId,
                        PatientName = $"{patient.FirstName} {patient.LastName}",
                        AppointmentDateTime = appointment.AppointmentDateTime
                    });
                }
            }

            return TypedResults.Ok(appointmentDtos);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> GetAppointmentById(IRepository<Appointment> appointmentRepository, IRepository<Doctor> doctorRepository, IRepository<Patient> patientRepository, int id)
        {
            var appointment = await appointmentRepository.GetById(id);
            if (appointment == null)
            {
                return TypedResults.NotFound(new { Message = "Appointment not found" });
            }

            var doctor = await doctorRepository.GetById(appointment.DoctorId);
            var patient = await patientRepository.GetById(appointment.PatientId);

            if (doctor == null)
            {
                return TypedResults.NotFound(new { Message = "Doctor not found" });
            }

            if (patient == null)
            {
                return TypedResults.NotFound(new { Message = "Patient not found" });
            }

            var appointmentDto = new AppointmentDTO
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                PatientId = appointment.PatientId,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                AppointmentDateTime = appointment.AppointmentDateTime
            };

            return TypedResults.Ok(appointmentDto);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> GetAppointmentsByDoctorId(IRepository<Appointment> appointmentRepository, IRepository<Doctor> doctorRepository, IRepository<Patient> patientRepository, int doctorId)
        {
            var doctor = await doctorRepository.GetById(doctorId);
            if (doctor == null)
            {
                return TypedResults.NotFound(new { Message = "Doctor not found" });
            }

            var appointments = (await appointmentRepository.Get()).Where(a => a.DoctorId == doctorId).ToList();

            if (!appointments.Any())
            {
                return TypedResults.NotFound(new { Message = "No appointments found for this doctor" });
            }

            var appointmentDtos = appointments.Select(a => new AppointmentDTO
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                PatientId = a.PatientId,
                AppointmentDateTime = a.AppointmentDateTime
            }).ToList();

            return TypedResults.Ok(appointmentDtos);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> GetAppointmentsByPatientId(IRepository<Appointment> appointmentRepository, IRepository<Patient> patientRepository, int patientId)
        {
            var patient = await patientRepository.GetById(patientId);
            if (patient == null)
            {
                return TypedResults.NotFound(new { Message = "Patient not found" });
            }

            var appointments = (await appointmentRepository.Get()).Where(a => a.PatientId == patientId).ToList();

            if (!appointments.Any())
            {
                return TypedResults.NotFound(new { Message = "No appointments found for this patient" });
            }

            var appointmentDtos = appointments.Select(a => new AppointmentDTO
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                PatientId = a.PatientId,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                AppointmentDateTime = a.AppointmentDateTime
            }).ToList();

            return TypedResults.Ok(appointmentDtos);
        }


        // Doctor endpoints
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public static async Task<IResult> CreateDoctor(IRepository<Doctor> repository, CreateDoctorDTO createDoctorDto)
        {
            if (createDoctorDto == null)
            {
                return TypedResults.BadRequest("Invalid data.");
            }

            var doctor = new Doctor
            {
                FirstName = createDoctorDto.FirstName,
                LastName = createDoctorDto.LastName
            };

            var createdDoctor = await repository.Insert(doctor);

            var createdDoctorDto = new DoctorDTO
            {
                Id = createdDoctor.Id,
                FirstName = createdDoctor.FirstName,
                LastName = createdDoctor.LastName
            };

            return TypedResults.Created($"/doctor/{createdDoctorDto.Id}", createdDoctorDto);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetDoctors(IRepository<Doctor> repository)
        {
            var doctors = await repository.Get();
            var doctorDtos = doctors.Select(d => new DoctorDTO
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName
            }).ToList();

            return TypedResults.Ok(doctorDtos);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> GetDoctorById(IRepository<Doctor> repository, int id)
        {
            var doctor = await repository.GetById(id);
            if (doctor == null)
            {
                return TypedResults.NotFound(new { Message = "Doctor not found" });
            }

            var doctorDto = new DoctorDTO
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName
            };

            return TypedResults.Ok(doctorDto);
        }

        // Patient endpoints
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public static async Task<IResult> CreatePatient(IRepository<Patient> repository, CreatePatientDTO createPatientDto)
        {
            if (createPatientDto == null)
            {
                return TypedResults.BadRequest("Invalid data.");
            }

            var patient = new Patient
            {
                FirstName = createPatientDto.FirstName,
                LastName = createPatientDto.LastName
            };

            var createdPatient = await repository.Insert(patient);

            var createdPatientDto = new PatientDTO
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
            var patientDtos = patients.Select(p => new PatientDTO
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

            var patientDto = new PatientDTO
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName
            };

            return TypedResults.Ok(patientDto);
        }
    }
}
