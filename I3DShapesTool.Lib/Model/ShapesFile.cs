﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3DShapesTool.Lib.Container;
using I3DShapesTool.Lib.Tools;
using Microsoft.Extensions.Logging;

namespace I3DShapesTool.Lib.Model
{
    /// <summary>
    /// Contains methods for loading and saving the contents of a .i3d.shapes file
    /// </summary>
    public class ShapesFile
    {
        /// <summary>
        /// Cipher seed
        /// </summary>
        public byte? Seed { get; set; }

        /// <summary>
        /// File version
        /// </summary>
        public short? Version { get; set; }

        /// <summary>
        /// File endian
        /// </summary>
        public Endian? Endian { get; set; }

        /// <summary>
        /// Entities in file
        /// </summary>
        public Entity[] Entities { get; set; }

        /// <summary>
        /// Parts in file
        /// </summary>
        public I3DPart[] Parts { get; set; }

        /// <summary>
        /// Shapes in file
        /// </summary>
        public IEnumerable<I3DShape> Shapes => Parts.OfType<I3DShape>();

        /// <summary>
        /// Splines in file
        /// </summary>
        public IEnumerable<Spline> Splines => Parts.OfType<Spline>();

        public ShapesFile()
        {
        }

        /// <summary>
        /// Read the .i3d.shapes contents from the input stream. The contents will be populated in the Parts array.
        /// </summary>
        /// <param name="inputStream">Stream of shapes file data</param>
        /// <param name="forceSeed">Force a specific seed instead of the one specified in the file header</param>
        /// <param name="strict">Abort reading and propagate any exceptions that pop up when parsing part data. If false, parts that failed to read will be ignored.</param>
        public void Load(Stream inputStream, byte? forceSeed = null, bool strict = false)
        {
            using(ShapesFileReader reader = new ShapesFileReader(inputStream, forceSeed))
            {
                Seed = reader.Header.Seed;
                Version = reader.Header.Version;
                Endian = reader.Endian;

                Entities = reader.GetEntities().ToArray();
                Parts = Entities
                    .Select(
                        (entityRaw, index) =>
                        {
                            try
                            {
                                I3DPart part = LoadPart(entityRaw, entityRaw.EntityType, reader.Endian, reader.Header.Version);

                                if(entityRaw.Type == 0 || entityRaw.Type > (int)EntityType.Last)
                                    Logger.Instance.LogInformation("Loaded part {name} with unknown type {type}.", part.Name, entityRaw.Type);

                                return part;
                            }
                            catch(Exception ex)
                            {
                                if(strict)
                                    throw;

                                Console.WriteLine(ex);
                                Logger.Instance.LogError("Failed to decode part {index}.", index);

                            // Failed to decode as the real part type, load it as a generic I3DPart instead so we at least can get hold of the binary data
                                try
                                {
                                    return LoadPart(entityRaw, EntityType.Unknown, reader.Endian, reader.Header.Version);
                                }
                                catch(Exception)
                                {
                                // We even failed to decode it as a generic part, just return null then instead.
                                    return null;
                                }
                            }
                        }
                    )
                    .Where(part => part != null)
                    .Cast<I3DPart>()
                    .ToArray();
            }
        }

        /// <summary>
        /// Write Parts data to the output stream as a .i3d.shapes file.
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <exception cref="ArgumentNullException">Thrown if Seed, Version or Parts is not set</exception>
        public void Write(Stream outputStream)
        {
            if(Seed == null || Version == null || Parts == null)
                throw new ArgumentNullException("Seed, Version and Parts must be set before saving.");

            using(ShapesFileWriter writer = new ShapesFileWriter(outputStream, (byte)Seed, (short)Version))
            {
                Entity[] entities = Parts.Select(part =>
                {
                    byte[] data;
                    using(MemoryStream ms = new MemoryStream())
                    {
                        using(EndianBinaryWriter bw = new EndianBinaryWriter(ms, writer.Endian))
                        {
                            part.Write(bw, (short)Version);
                            bw.Flush();
                            data = ms.ToArray();
                        }
                    }

                    return new Entity((int)part.Type, data.Length, data);
                }).ToArray();

                writer.SaveEntities(entities);
            }
        }

        private static I3DPart LoadPart(Entity entityRaw, EntityType partType, Endian endian, short version)
        {
            I3DPart part;
            switch(partType)
            {
                case EntityType.Shape:
                    part = new I3DShape();
                    break;
                case EntityType.Spline:
                    part = new Spline();
                    break;
                case EntityType.Unknown:
                    part = new I3DPart();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            using(MemoryStream stream = new MemoryStream(entityRaw.Data))
            {
                using(EndianBinaryReader reader = new EndianBinaryReader(stream, endian))
                {
                    part.Read(reader, version);
                }
            }

            return part;
        }
    }
}
