using System;
using System.Collections.Generic;
using System.Text;

namespace FTN.Common
{
	
	public enum DMSType : short
	{		
		MASK_TYPE							= unchecked((short)0xFFFF),

        CONNECTNODECONT                 = 0x0001,       // ConnectivityNodeContainer
        CONNECTNODE                     = 0x0002,       // ConnectivityNode
        ENERGSOURCE                     = 0x0003,       // EnergySource
        ACLINESEGMENT                   = 0x0004,
        BREAKER                         = 0x0005,       
        ENERGCONSUMER                   = 0x0006,        // EnergyConsumer
        TERMINAL                        = 0x0007,
        DISCRETE                        = 0x0008,
        ANALOG                          = 0x0009,
	}

    [Flags]
	public enum ModelCode : long
	{
        IDOBJ                           = 0x1000000000000000,
        IDOBJ_GID                       = 0x1000000000000104,
        IDOBJ_MRID                      = 0x1000000000000207,
        IDOBJ_NAME                      = 0x1000000000000307,

        PSR                             = 0x1100000000000000,
        PSR_MEASUREMENTS                = 0x1100000000000119,

        MEASUREMENT                     = 0x1200000000000000,
        MEASUREMENT_DIRECTION           = 0x120000000000010a,
        MEASUREMENT_TYPE                = 0x1200000000000207,
        MEASUREMENT_UNITSYMB            = 0x120000000000030a,
        MEASUREMENT_PSR                 = 0x1200000000000409,

        TERMINAL                        = 0x1300000000070000,
        TERMINAL_CONDEQUIP              = 0x1300000000070109,       // Terminal.ConductingEquipment
        TERMINAL_CONNECTNODE            = 0x1300000000070209,       // Terminal.ConnectivityNode

        CONNECTNODE                     = 0x1400000000020000,
        CONNECTNODE_CONNECTNODECONT     = 0x1400000000020109,
        CONNECTNODE_TERMINALS           = 0x1400000000020219,

        EQUIPMENT                       = 0x1110000000000000,
        EQUIPMENT_NORMINSERV            = 0x1110000000000101,       // Equipment.normallyInService

        CONNECTNODECONT                 = 0x1120000000010000,
        CONNECTNODECONT_CONNECTNODES    = 0x1120000000010119,

        DISCRETE                        = 0x1210000000080000,
        DISCRETE_MINVAL                 = 0x1210000000080103,
        DISCRETE_MAXVAL                 = 0x1210000000080203,
        DISCRETE_NORMVAL                = 0x1210000000080303,
        DISCRETE_VALIDCOMMANDS          = 0x121000000008041a,
        DISCRETE_VALIDSTATES            = 0x121000000008051a,

        ANALOG                          = 0x1220000000090000,
        ANALOG_MINVAL                   = 0x1220000000090105,
        ANALOG_MAXVAL                   = 0x1220000000090205,
        ANALOG_NORMVAL                  = 0x1220000000090305,

        CONDUCTEQUIP                    = 0x1111000000000000,       // ConductingEquipment
        CONDUCTEQUIP_TERMINALS          = 0x1111000000000119,

        SWITCH                          = 0x1111100000000000,
        SWITCH_NORMOPEN                 = 0x1111100000000101,
        SWITCH_ONCOUNT                  = 0x1111100000000203,
        SWITCH_ONDATE                   = 0x1111100000000308,

        CONDUCTOR                       = 0x1111200000000000,
        CONDUCTOR_LEN                   = 0x1111200000000105,

        ENERGSOURCE                     = 0x1111300000030000,
        ENERGSOURCE_ACTPOW              = 0x1111300000030105,       // EnergySource.ActivePower
        ENERGSOURCE_NOMVOLT             = 0x1111300000030205,       // EnergySource.NominalVoltage

        ENERGCONSUMER                   = 0x1111400000060000,
        ENERGCONSUMER_QFIXED            = 0x1111400000060105,
        ENERGCONSUMER_PFIXED            = 0x1111400000060205,

        PROTSWITCH                      = 0x1111110000000000,       // ProtectedSwitch

        ACLINESEGMENT                   = 0x1111210000040000,

        BREAKER                         = 0x1111111000050000,

        //      IDOBJ								= 0x1000000000000000,
        //IDOBJ_GID							= 0x1000000000000104,
        //IDOBJ_DESCRIPTION					= 0x1000000000000207,
        //IDOBJ_MRID							= 0x1000000000000307,
        //IDOBJ_NAME							= 0x1000000000000407,	

        //PSR									= 0x1100000000000000,
        //PSR_CUSTOMTYPE						= 0x1100000000000107,
        //PSR_LOCATION						= 0x1100000000000209,

        //BASEVOLTAGE							= 0x1200000000010000,
        //BASEVOLTAGE_NOMINALVOLTAGE			= 0x1200000000010105,
        //BASEVOLTAGE_CONDEQS					= 0x1200000000010219,

        //LOCATION							= 0x1300000000020000,
        //LOCATION_CORPORATECODE				= 0x1300000000020107,
        //LOCATION_CATEGORY					= 0x1300000000020207,
        //LOCATION_PSRS						= 0x1300000000020319,

        //WINDINGTEST							= 0x1400000000050000,
        //WINDINGTEST_LEAKIMPDN				= 0x1400000000050105,
        //WINDINGTEST_LOADLOSS				= 0x1400000000050205,
        //WINDINGTEST_NOLOADLOSS				= 0x1400000000050305,
        //WINDINGTEST_PHASESHIFT				= 0x1400000000050405,
        //WINDINGTEST_LEAKIMPDN0PERCENT		= 0x1400000000050505,
        //WINDINGTEST_LEAKIMPDNMINPERCENT		= 0x1400000000050605,
        //WINDINGTEST_LEAKIMPDNMAXPERCENT		= 0x1400000000050705,
        //WINDINGTEST_POWERTRWINDING			= 0x1400000000050809,

        //EQUIPMENT							= 0x1110000000000000,
        //EQUIPMENT_ISUNDERGROUND				= 0x1110000000000101,
        //EQUIPMENT_ISPRIVATE					= 0x1110000000000201,		

        //CONDEQ								= 0x1111000000000000,
        //CONDEQ_PHASES						= 0x111100000000010a,
        //CONDEQ_RATEDVOLTAGE					= 0x1111000000000205,
        //CONDEQ_BASVOLTAGE					= 0x1111000000000309,

        //POWERTR								= 0x1112000000030000,
        //POWERTR_FUNC						= 0x111200000003010a,
        //POWERTR_AUTO						= 0x1112000000030201,
        //POWERTR_WINDINGS					= 0x1112000000030319,

        //TRWINDING						    = 0x1111100000040000,
        //TRWINDING_CONNTYPE				    = 0x111110000004010a,
        //TRWINDING_GROUNDED				    = 0x1111100000040201,
        //TRWINDING_RATEDS				    = 0x1111100000040305,
        //TRWINDING_RATEDU				    = 0x1111100000040405,
        //TRWINDING_WINDTYPE				    = 0x111110000004050a,
        //TRWINDING_PHASETOGRNDVOLTAGE	    = 0x1111100000040605,
        //TRWINDING_PHASETOPHASEVOLTAGE	    = 0x1111100000040705,
        //TRWINDING_POWERTRW				    = 0x1111100000040809,
        //TRWINDING_TESTS				        = 0x1111100000040919,
    }

    [Flags]
	public enum ModelCodeMask : long
	{
		MASK_TYPE			 = 0x00000000ffff0000,
		MASK_ATTRIBUTE_INDEX = 0x000000000000ff00,
		MASK_ATTRIBUTE_TYPE	 = 0x00000000000000ff,

		MASK_INHERITANCE_ONLY = unchecked((long)0xffffffff00000000),
		MASK_FIRSTNBL		  = unchecked((long)0xf000000000000000),
		MASK_DELFROMNBL8	  = unchecked((long)0xfffffff000000000),		
	}																		
}

