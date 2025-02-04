namespace StarControl.Config;

/// <summary>
/// Simple value-equality comparer targeting mutable configuration types.
/// </summary>
/// <remarks>
/// The interface has the same members as <see cref="IEquatable{T}"/>, but the distinction is made
/// intentionally in order to avoid triggering the value-equality logic where it isn't specifically
/// requested, because the underlying type is assumed to be mutable and thus a poor candidate for
/// dictionary keys and other instances where <see cref="Object.Equals(Object)"/> and
/// <see cref="Object.GetHashCode"/> should normally also be overridden.
/// </remarks>
/// <typeparam name="T">Type to compare for equality.</typeparam>
public interface IConfigEquatable<in T>
{
    /// <summary>
    /// Checks if an object instance is equal to the current instance.
    /// </summary>
    /// <param name="other">The instance to compare.</param>
    /// <returns><c>true</c> if <paramref name="other"/> has a value equal to this instance,
    /// otherwise <c>false</c>.</returns>
    bool Equals(T? other);
}
