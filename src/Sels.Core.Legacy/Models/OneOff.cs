using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Models
{
    /// <summary>
    /// Represents a type that can either contain a value of type <typeparamref name="T0"/> or <typeparamref name="T1"/> but not both.
    /// </summary>
    /// <typeparam name="T0">The type of the first value.</typeparam>
    /// <typeparam name="T1">The type of the second value.</typeparam>
    public class OneOff<T0, T1>
    {
        /// <summary>
        /// If <see cref="ValueOne"/> is set.
        /// </summary>
        public bool ValueOneSet { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="OneOff{T0, T1}"/> class with the specified values.
        /// </summary>
        public T0 ValueOne { get; }
        /// <summary>
        /// If <see cref="ValueTwo"/> is set.
        /// </summary>
        public bool ValueTwoSet { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="OneOff{T0, T1}"/> class with the specified values.
        /// </summary>
        public T1 ValueTwo { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOff{T0, T1}"/> class with the specified value of type <typeparamref name="T0"/>.
        /// </summary>
        /// <param name="valueOne">The value of type <typeparamref name="T0"/> to set.</param>
        public OneOff(T0 valueOne)
        {
            ValueOne = valueOne;
            ValueOneSet = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OneOff{T0, T1}"/> class with the specified value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="valueTwo">The value of type <typeparamref name="T1"/> to set.</param>
        public OneOff(T1 valueTwo)
        {
            ValueTwo = valueTwo;
            ValueTwoSet = true;
        }

        /// <summary>
        /// Defines an implicit conversion from <typeparamref name="T0"/> to <see cref="OneOff{T0, T1}"/>.
        /// </summary>
        /// <param name="value">The value of type <typeparamref name="T0"/> to convert.</param>
        public static implicit operator OneOff<T0, T1>(T0 value) => new OneOff<T0, T1>(value);
        /// <summary>
        /// Defines an implicit conversion from <typeparamref name="T1"/> to <see cref="OneOff{T0, T1}"/>.
        /// </summary>
        /// <param name="value">The value of type <typeparamref name="T1"/> to convert.</param>
        public static implicit operator OneOff<T0, T1>(T1 value) => new OneOff<T0, T1>(value);
        /// <summary>
        /// Defines an equality operator for <see cref="OneOff{T0, T1}"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if the operands are equal; otherwise, false.</returns>
        public static bool operator ==(OneOff<T0, T1> left, OneOff<T0, T1> right) => left?.Equals(right) ?? false;
        /// <summary>
        /// Defines an inequality operator for <see cref="OneOff{T0, T1}"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if the operands are not equal; otherwise, false.</returns>
        public static bool operator !=(OneOff<T0, T1> left, OneOff<T0, T1> right) => !(left == right);

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is OneOff<T0, T1> other)
            {
                if (ValueOneSet && other.ValueOneSet)
                {
                    return EqualityComparer<T0>.Default.Equals(ValueOne, other.ValueOne);
                }
                else if (ValueTwoSet && other.ValueTwoSet)
                {
                    return EqualityComparer<T1>.Default.Equals(ValueTwo, other.ValueTwo);
                }
            }
            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (ValueOneSet)
            {
                return EqualityComparer<T0>.Default.GetHashCode(ValueOne);
            }
            else if (ValueTwoSet)
            {
                return EqualityComparer<T1>.Default.GetHashCode(ValueTwo);
            }
            return 0;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            if (ValueOneSet)
            {
                return ValueOne?.ToString() ?? "null";
            }
            else if (ValueTwoSet)
            {
                return ValueTwo?.ToString() ?? "null";
            }
            return "No value set";
        }
    }
}
