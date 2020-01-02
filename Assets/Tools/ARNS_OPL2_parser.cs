﻿#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ARNS_OPL2ParserScriptableWizard : UnityEditor.ScriptableWizard
{
    public UnityEngine.Object opl2Asset = null;

    [Serializable]
    public class ARNS_OPL2
    {
        public class ARNS_OPL2Vec3
        {
            public float x = 0;
            public float y = 0;
            public float z = 0;

            public ARNS_OPL2Vec3(BinaryReader binaryReader)
            {
                x = binaryReader.ReadSingle();
                y = binaryReader.ReadSingle();
                z = binaryReader.ReadSingle();
            }
        }

        public class ARNS_OPL2Vec4
        {
            public float x = 0;
            public float y = 0;
            public float z = 0;
            public float w = 0;

            public ARNS_OPL2Vec4(BinaryReader binaryReader)
            {
                x = binaryReader.ReadSingle();
                y = binaryReader.ReadSingle();
                z = binaryReader.ReadSingle();
                w = binaryReader.ReadSingle();
            }
        }

        public class ARNS_OPL2Entry
        {
            public class ARNS_OPL2Object
            {
                public ARNS_OPL2Vec3 position;
                public ARNS_OPL2Vec4 rotation;
                public ARNS_OPL2Vec3 scale;
                public float scaleW;
                public byte[] unkA;
                public int unkB;
                public byte[] unkC = new byte[4];
                public ARNS_OPL2Vec4 padding1;
                public ARNS_OPL2Vec4 padding2;

                public ARNS_OPL2Object(BinaryReader binaryReader)
                {
                    position = new ARNS_OPL2Vec3(binaryReader);
                    rotation = new ARNS_OPL2Vec4(binaryReader);
                    scale = new ARNS_OPL2Vec3(binaryReader);
                    scaleW = binaryReader.ReadSingle();
                    unkA = binaryReader.ReadBytes(4);
                    unkB = binaryReader.ReadInt32();
                    unkC = binaryReader.ReadBytes(unkB);
                    padding1 = new ARNS_OPL2Vec4(binaryReader);
                    padding2 = new ARNS_OPL2Vec4(binaryReader);
                }
            }

            public int objNameLength;
            public char[] objName;
            public uint unknown;
            public uint padding1;
            public uint padding2;
            public uint padding3;
            public uint padding4;
            public uint objCount;
            public List<ARNS_OPL2Object> objects = new List<ARNS_OPL2Object>();

            public ARNS_OPL2Entry(BinaryReader binaryReader)
            {
                objNameLength = binaryReader.ReadInt32();
                objName = binaryReader.ReadChars(objNameLength);
                unknown = binaryReader.ReadUInt32();
                padding1 = binaryReader.ReadUInt32();
                padding2 = binaryReader.ReadUInt32();
                padding3 = binaryReader.ReadUInt32();
                padding4 = binaryReader.ReadUInt32();
                objCount = binaryReader.ReadUInt32();
                for (int i = 0; i < objCount; i++)
                {
                    ARNS_OPL2Object opl2_obj = new ARNS_OPL2Object(binaryReader);
                    objects.Add(opl2_obj);
                }

            }
        }

        public uint entryCount;
        public List<ARNS_OPL2Entry> entries = new List<ARNS_OPL2Entry>();

        public ARNS_OPL2(BinaryReader binaryReader)
        {
            entryCount = binaryReader.ReadUInt32();
            for (int i = 0; i < entryCount; i++)
            {
                ARNS_OPL2Entry opl2_entry = new ARNS_OPL2Entry(binaryReader);
                entries.Add(opl2_entry);
            }
        }
    }

    class BinaryReaderBigEndian : BinaryReader
    {
        public BinaryReaderBigEndian(Stream stream) : base(stream) { }

        public override short ReadInt16()
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public override int ReadInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public override long ReadInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public override ushort ReadUInt16()
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public override uint ReadUInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public override ulong ReadUInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

        public override float ReadSingle()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }
        public override double ReadDouble()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }
    }

    private void ParseEntities()
    {
        if(opl2Asset)
        {
            String path = AssetDatabase.GetAssetPath(opl2Asset);
            {
                Stream stream = new FileStream(path, FileMode.Open);
                if (stream != null)
                {
                    BinaryReader binaryReader = new BinaryReaderBigEndian(stream);
                    ARNS_OPL2 opl2 = new ARNS_OPL2(binaryReader);

                    for (int i = 0; i < opl2.entries.Count; i++)
                    {
                        string objName = new string(opl2.entries[i].objName);
                        for (int j = 0; j < opl2.entries[i].objects.Count; j++)
                        {
                            ARNS_OPL2.ARNS_OPL2Entry.ARNS_OPL2Object current_opl2_object = opl2.entries[i].objects[j];

                            GameObject gameObject = new GameObject();
                            gameObject.name = objName + "_" + j.ToString();

                            gameObject.transform.localPosition = new Vector3(
                                current_opl2_object.position.x,
                                current_opl2_object.position.y,
                                current_opl2_object.position.z) * 0.01f;

                            gameObject.transform.localRotation = new Quaternion(
                                current_opl2_object.rotation.x,
                                current_opl2_object.rotation.y,
                                current_opl2_object.rotation.z,
                                current_opl2_object.rotation.w);

                            gameObject.transform.localScale = new Vector3(
                                current_opl2_object.scale.x,
                                current_opl2_object.scale.y,
                                current_opl2_object.scale.z);
                        }
                    }
                    stream.Close();
                }
            }
        }
    }

    [UnityEditor.MenuItem("Tools/Ar Nosurge Tools/Parse OPL2 File")]
    static void CreateWizard()
    {
        UnityEditor.ScriptableWizard.DisplayWizard<ARNS_OPL2ParserScriptableWizard>("Parse Ar Nosurge OPL2 File", "Apply");
    }

    void OnWizardCreate()
    {
        ParseEntities();
    }

    void OnWizardUpdate()
    {

    }
};

#endif