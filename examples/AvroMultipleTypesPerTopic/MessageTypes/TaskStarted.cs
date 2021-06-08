// ------------------------------------------------------------------------------
// <auto-generated>
//    Generated by avrogen, version 1.10.0.0
//    Changes to this file may cause incorrect behavior and will be lost if code
//    is regenerated
// </auto-generated>
// ------------------------------------------------------------------------------
namespace MessageTypes
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Avro;
	using Avro.Specific;
	
	/// <summary>
	/// The event raised when a task is started.
	/// </summary>
	public partial class TaskStarted : ISpecificRecord
	{
		public static Schema _SCHEMA = Avro.Schema.Parse(@"{""type"":""record"",""name"":""TaskStarted"",""namespace"":""MessageTypes"",""fields"":[{""name"":""Id"",""doc"":""The id of the task"",""type"":{""type"":""string"",""logicalType"":""uuid""}},{""name"":""StartedDate"",""doc"":""The date when the task started"",""type"":{""type"":""long"",""logicalType"":""timestamp-millis""}},{""name"":""StartedOn"",""doc"":""The worker starting the task"",""type"":""string""}]}");
		/// <summary>
		/// The id of the task
		/// </summary>
		private System.Guid _Id;
		/// <summary>
		/// The date when the task started
		/// </summary>
		private System.DateTime _StartedDate;
		/// <summary>
		/// The worker starting the task
		/// </summary>
		private string _StartedOn;
		public virtual Schema Schema
		{
			get
			{
				return TaskStarted._SCHEMA;
			}
		}
		/// <summary>
		/// The id of the task
		/// </summary>
		public System.Guid Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				this._Id = value;
			}
		}
		/// <summary>
		/// The date when the task started
		/// </summary>
		public System.DateTime StartedDate
		{
			get
			{
				return this._StartedDate;
			}
			set
			{
				this._StartedDate = value;
			}
		}
		/// <summary>
		/// The worker starting the task
		/// </summary>
		public string StartedOn
		{
			get
			{
				return this._StartedOn;
			}
			set
			{
				this._StartedOn = value;
			}
		}
		public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.Id;
			case 1: return this.StartedDate;
			case 2: return this.StartedOn;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.Id = (System.Guid)fieldValue; break;
			case 1: this.StartedDate = (System.DateTime)fieldValue; break;
			case 2: this.StartedOn = (System.String)fieldValue; break;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
	}
}
