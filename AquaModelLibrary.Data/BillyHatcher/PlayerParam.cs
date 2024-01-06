namespace AquaModelLibrary.Data.BillyHatcher
{
    public struct PlayerParam
    {
        public byte bt_00;
        public byte bt_01;
        public byte bt_02;
        public byte bt_03;
        public ushort usht_04;
        public ushort usht_06;
        public byte bt_08;
        public byte bt_09;
        public byte bt_0A;
        public byte bt_0B;
        public ushort usht_0C;
        public ushort usht_0E;

        public ushort usht_10;
        public ushort usht_12;
        public ushort usht_14;
        public ushort usht_16;
        public ushort usht_18;
        public ushort usht_1A;
        public ushort usht_1C;
        public ushort usht_1E;

        public ushort usht_20;
        public ushort usht_22;
        public ushort usht_24;
        public ushort usht_26;
        public ushort usht_28;
        public ushort usht_2A;
        public ushort usht_2C;
        public ushort usht_2E;

        public ushort usht_30;
        public ushort usht_32;
        public ushort usht_34;
        public ushort usht_36;
        public float flt_38;
        /// <summary>
        /// 0x3C
        /// The higher it is, the harder it is to move at all.
        /// Only for eggless Billy.
        /// </summary>
        public float accelerationThing;

        /// <summary>
        /// 0x40
        /// The higher it is, the harder it is to go fast. Range is meant to be 0-1 with above 1 meaning you go the slow walk speed. Low values will result in it being basically impossible to walk slow.
        /// Only for eggless Billy.
        /// </summary>
        public float floorGrip;
        /// <summary>
        /// 0x44
        /// Unknown
        /// </summary>
        public float flt_44;
        /// <summary>
        /// 0x48
        /// Air horizontal speed thing.
        /// Only for eggless Billy.
        /// </summary>
        public float airHorizontalSpeedThing;
        /// <summary>
        /// 0x4C
        /// Controls how high billy can jump, partly.
        ///Only for eggless Billy.
        /// </summary>
        public float airJumpHeightThing;

        /// <summary>
        /// 0x50
        /// Slow walk speed.
        /// Only for eggless Billy.
        /// </summary>
        public float slowWalkSpeed;
        /// <summary>
        /// 0x54
        /// Slow walk animation speed.
        /// Only for eggless Billy.
        /// </summary>
        public float slowWalkAnimSpeed;
        /// <summary>
        /// 0x58
        /// Turn speed when on ground.
        /// Only for eggless Billy.
        /// </summary>
        public float groundTurnSpeed;
        /// <summary>
        /// 0x5C
        /// Max run speed without an egg.
        /// Only for eggless Billy.
        /// </summary>
        public float runSpeed;

        /// <summary>
        /// 0x60
        /// Billy's acceleration speed.
        /// </summary>
        public float acceleration;
        /// <summary>
        /// 0x64
        /// Run animation speed.
        /// Only for eggless Billy.
        /// </summary>
        public float runAnimSpeed;
        /// <summary>
        /// 0x68
        /// Unknown.
        /// </summary>
        public float flt_68;
        /// <summary>
        /// 0x6C
        /// Max forward speed of jumps
        /// </summary>
        public float jumpMaxForwardSpeed;

        /// <summary>
        /// 0x70
        /// The max height Billy's jump can reach.
        /// </summary>
        public float maxJumpHeight;
        /// <summary>
        /// 0x74
        /// Increasing this increases the amount of acceleration you can have when you try to move while already in the air.
        /// </summary>
        public float aerialControl;
        /// <summary>
        /// 0x78
        /// Amount of time holding the button will keep accelerating you upwards.
        /// </summary>
        public float jumpAccelerationTime;
        /// <summary>
        /// 0x7C
        /// Gravity!
        /// Only for eggless Billy.
        /// </summary>
        public float gravity;

        public float flt_80;
        public float flt_84;
        public float flt_88;
        public float flt_8C;

        public float flt_90;
        public float flt_94;
        public float flt_98;
        public float flt_9C;

        public float flt_A0;
        public float flt_A4;
        /// <summary>
        /// 0xA8
        /// Unknown, but putting it above 1 halts ground movement. 
        /// </summary>
        public float flt_A8;
        public float flt_AC;

        public float flt_B0;
        public float flt_B4;
        public float flt_B8;
        public float flt_BC;

        public float flt_C0;
        public float flt_C4;
        public float flt_C8;
        public float flt_CC;

        public float flt_D0;
        public float flt_D4;
        public float flt_D8;
        /// <summary>
        /// 0xDC
        /// Gravity during bounce jump.
        /// </summary>
        public float bounceJumpGravity;

        /// <summary>
        /// 0xE0
        /// Force applied to bounce jump.
        /// </summary>
        public float bounceJumpForce;
        /// <summary>
        /// 0xE4
        /// Unknown factor related to bounce jump.
        /// </summary>
        public float bounceJumpSomething;
        /// <summary>
        /// 0xE8
        /// Rolling Jump gravity
        /// </summary>
        public float rollingJumpGravity;
        /// <summary>
        /// 0xEC
        /// Rolling Jump forward force
        /// </summary>
        public float rollingJumpForwardForce;

        /// <summary>
        /// 0xF0
        /// Rolling Jump upward force
        /// </summary>
        public float rollingJumpUpwardForce;
        public float flt_F4;
        /// <summary>
        /// 0xF8
        /// Affects the deceleration when landing from aerial rolling
        /// </summary>
        public float rollingLandingDeceleration;
        public float flt_FC;

        public float flt_100;
        public float flt_104;
        /// <summary>
        /// 0x108
        /// Seems capped by something else, but affects egg shoot speed
        /// </summary>
        public float eggShootForce;
        public float flt_10C;

        public float flt_110;
        public float flt_114;
        public float flt_118;
        public float eggGravity;
    }
}
