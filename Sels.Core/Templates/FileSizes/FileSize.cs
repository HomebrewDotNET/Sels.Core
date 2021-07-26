using Sels.Core.Extensions;
using Sels.Core.Extensions.Calculation;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using SingleByte = Sels.Core.Components.FileSizes.Byte.SingleByte;

namespace Sels.Core.Templates.FileSizes
{
    /// <summary>
    /// Base class for creating file sizes and allows for easy conversion between other filesizes.
    /// </summary>
    public abstract class FileSize : IComparable<FileSize>, IEquatable<FileSize>
    {
        // Constants
        public const int ByteToBitMultiplier = 8;
        public const int DefaultUnitSize = 1000;

        // Fields
        protected long _byteSize;
        protected decimal _size;

        // Properties
        /// <summary>
        /// Divide size we need to get to the next size. For example to go from 1 byte to 1 kilobyte we divide by 1000 or 1024.
        /// </summary>
        public virtual int UnitSize => DefaultUnitSize;

        /// <summary>
        /// Full display name of this file size.
        /// </summary>
        public virtual string DisplayName => this.GetType().Name.ToLower();

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long ByteSize
        {
            get
            {
                return _byteSize;
            }

            set
            {
                value.ValidateArgumentLargerOrEqual(nameof(ByteSize), 0);
                _byteSize = value;
                _size = GetFileSize(value);
            }
        }

        /// <summary>
        /// File size.
        /// </summary>
        public decimal Size
        {
            get
            {
                return _size;
            }

            set
            {
                value.ValidateArgumentLargerOrEqual(nameof(Size), 0);
                _size = value;
                _byteSize = GetByteFileSize(value);
            }
        }

        public FileSize()
        {

        }

        public FileSize(long byteSize) : base()
        {
            byteSize.ValidateArgumentLargerOrEqual(nameof(byteSize), 0);
            ByteSize = byteSize;
        }

        public FileSize(decimal size) : base()
        {
            size.ValidateArgumentLargerOrEqual(nameof(size), 0);
            Size = size;
        }

        /// <summary>
        /// Converts this file size to file size <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">File size to convert to</typeparam>
        /// <returns>Converted file size</returns>
        public virtual T ToSize<T>() where T : FileSize, new()
        {
            return CreateFromBytes<T>(ByteSize);
        }

        /// <summary>
        /// Converts this file size to file size <paramref name="type"/>.
        /// </summary>
        /// <param name="type">File size to convert to</param>
        /// <returns>Converted file size</returns>
        public virtual FileSize ToSize(Type type) 
        {
            type.ValidateArgument(nameof(type));
            type.ValidateArgumentAssignableTo(nameof(type), typeof(FileSize));
            type.ValidateArgumentCanBeContructedWith(nameof(type));

            return CreateFromBytes(ByteSize, type);
        }

        protected virtual decimal GetFileSize(long byteSize)
        {
            // Convert from bytes first
            var newSize = SizeMultiplier != 0 ? byteSize.DivideBy(UnitSize, SizeMultiplier) : byteSize;

            // Convert byte to bit
            newSize = IsByteSize ? newSize : newSize * ByteToBitMultiplier;

            return newSize;
        }

        protected virtual long GetByteFileSize(decimal size)
        {
            // Convert to bit to byte format
            size = IsByteSize ? size : size / ByteToBitMultiplier;

            // Convert to bytes
            var newByteSize = SizeMultiplier != 0 ? size.MultiplyBy(UnitSize, SizeMultiplier) : size;

            return newByteSize.ConvertTo<long>();
        }

        // Abstractions
        /// <summary>
        /// The abbreviation for this filesize.
        /// </summary>
        public abstract string Abbreviation { get; }

        /// <summary>
        /// True if sized in bytes, false if sized in bits.
        /// </summary>
        public abstract bool IsByteSize { get; }

        /// <summary>
        /// How many times we need to use <see cref="UnitSize"/> to convert from bytes to current size.
        /// </summary>
        protected abstract int SizeMultiplier { get; }

        public override string ToString()
        {
            return Size + Abbreviation;
        }

        /// <summary>
        /// Displays this file size in bytes.
        /// </summary>
        /// <returns></returns>
        public string ToByteString()
        {
            return Size + SingleByte.FileSizeAbbreviation;
        }

        /// <summary>
        /// Displays the current file size with the display name instead of the abbreviation.
        /// </summary>
        /// <returns></returns>
        public string ToDisplayString()
        {
            return $"{Size} {DisplayName}";
        }

        /// <summary>
        /// Displays this file size in bytes with the display name instead of the abbreviation.
        /// </summary>
        /// <returns></returns>
        public string ToDisplayByteString()
        {
            return $"{Size} {SingleByte.FileSizeAbbreviation}";
        }

        // Statics
        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> with <paramref name="bytes"/> as the <see cref="FileSize.ByteSize"/>.
        /// </summary>
        /// <typeparam name="T">File size to create</typeparam>
        /// <param name="bytes">Byte size of new file size</param>
        /// <returns>New file size</returns>
        public static T CreateFromBytes<T>(long bytes) where T : FileSize, new()
        {
            bytes.ValidateArgumentLargerOrEqual(nameof(bytes), 0);

            return CreateFromBytes(bytes, typeof(T)).As<T>();
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> with <paramref name="size"/> as the <see cref="FileSize.Size"/>.
        /// </summary>
        /// <typeparam name="T">File size to create</typeparam>
        /// <param name="size">Size of new file size</param>
        /// <returns>New file size</returns>
        public static T CreateFromSize<T>(decimal size) where T : FileSize, new()
        {
            size.ValidateArgumentLargerOrEqual(nameof(size), 0);

            return CreateFromSize(size, typeof(T)).As<T>();
        }

        /// <summary>
        /// Creates a new instance of <paramref name="type"/> with <paramref name="bytes"/> as the <see cref="FileSize.ByteSize"/>.
        /// </summary>
        /// <param name="type">File size to create</param>
        /// <param name="bytes">Byte size of new file size</param>
        /// <returns>New file size</returns>
        public static FileSize CreateFromBytes(long bytes, Type type)
        {
            bytes.ValidateArgumentLargerOrEqual(nameof(bytes), 0);
            type.ValidateArgument(nameof(type));
            type.ValidateArgumentAssignableTo(nameof(type), typeof(FileSize));
            type.ValidateArgumentCanBeContructedWith(nameof(type));

            var fileSize = type.Construct<FileSize>();
            fileSize.ByteSize = bytes;

            return fileSize;
        }

        /// <summary>
        /// Creates a new instance of <paramref name="type"/> with <paramref name="size"/> as the <see cref="FileSize.Size"/>.
        /// </summary>
        /// <param name="type">File size to create</param>
        /// <param name="size">Size of new file size</param>
        /// <returns>New file size</returns>
        public static FileSize CreateFromSize(decimal size, Type type) 
        {
            size.ValidateArgumentLargerOrEqual(nameof(size), 0);
            type.ValidateArgument(nameof(type));
            type.ValidateArgumentAssignableTo(nameof(type), typeof(FileSize));
            type.ValidateArgumentCanBeContructedWith(nameof(type));

            var fileSize = type.Construct<FileSize>();
            fileSize.Size = size;

            return fileSize;
        }

        #region Operations
        public static bool operator ==(FileSize fileSize, FileSize otherFileSize)
        {
            return fileSize?.ByteSize == otherFileSize?.ByteSize;
        }

        public static bool operator !=(FileSize fileSize, FileSize otherFileSize)
        {
            return fileSize?.ByteSize != otherFileSize?.ByteSize;
        }

        public static bool operator <(FileSize fileSize, FileSize otherFileSize)
        {
            return fileSize?.ByteSize < otherFileSize?.ByteSize;
        }

        public static bool operator >(FileSize fileSize, FileSize otherFileSize)
        {
            return fileSize?.ByteSize > otherFileSize?.ByteSize;
        }

        public static bool operator <=(FileSize fileSize, FileSize otherFileSize)
        {
            return fileSize?.ByteSize <= otherFileSize?.ByteSize;
        }

        public static bool operator >=(FileSize fileSize, FileSize otherFileSize)
        {
            return fileSize?.ByteSize >= otherFileSize?.ByteSize;
        }

        public static SingleByte operator +(FileSize fileSize, FileSize otherFileSize)
        {
            var bytes = fileSize.HasValue() ? fileSize.ByteSize : 0;
            var otherBytes = otherFileSize.HasValue() ? otherFileSize.ByteSize : 0;

            return CreateFromBytes<SingleByte>(bytes + otherBytes);
        }

        public static SingleByte operator -(FileSize fileSize, FileSize otherFileSize)
        {
            var bytes = fileSize.HasValue() ? fileSize.ByteSize : 0;
            var otherBytes = otherFileSize.HasValue() ? otherFileSize.ByteSize : 0;
            var newBytes = bytes - otherBytes;

            return CreateFromBytes<SingleByte>(newBytes > 0 ? newBytes : 0);
        }

        public override int GetHashCode()
        {
            return ByteSize.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is FileSize objFileSize && ByteSize.Equals(objFileSize.ByteSize);
        }

        public int CompareTo(FileSize other)
        {
            return ByteSize.CompareTo(other?.ByteSize ?? 0);
        }

        public bool Equals(FileSize other)
        {
            return Equals(other.AsOrDefault<object>());
        }
        #endregion
    }
}
