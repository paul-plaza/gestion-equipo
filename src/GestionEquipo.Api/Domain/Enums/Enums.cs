namespace GestionEquipo.Api.Domain.Enums;

public enum TipoPersona { Interno, Externo }

public enum EstadoPersona { Activo, Inactivo }

public enum RolProyecto { Desarrollador, ScrumMaster, Arquitecto }

public enum PorcentajeDedicacion { Veinticinco = 25, Cincuenta = 50, Cien = 100 }

public enum EstadoProyecto { PorIniciar, EnCurso, Atrasado, Finalizado }

public enum TipoHoraExtra { Proyecto, SoporteApoyo, Pruebas }

public enum EstadoHoraExtra { Pendiente, Aprobado, Recuperado, NoRecuperable }

public enum EstadoPermiso { Pendiente, Aprobado, Rechazado }

public enum NivelDominio { Basico, Intermedio, Avanzado }

public enum RolSistema { JefeDesarrollo, JefeArea, Persona }
