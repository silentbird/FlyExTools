namespace pure.utils.task
{
  public struct InvalidateCache
  {
    private InvalidateType _mask;

    public InvalidateCache(InvalidateType val = InvalidateType.Nothing) => _mask = val;

    public void Add(InvalidateType property = InvalidateType.All) => _mask |= property;

    public bool Contains(InvalidateType property, InvalidateType args)
    {
      return Contains(property) || Contains(args);
    }

    public void Remove(InvalidateType property) => _mask &= ~property;

    public bool Contains(InvalidateType property) => (_mask & property) != 0;

    public bool Empty() => _mask == InvalidateType.Nothing;

    public void Clear() => _mask = InvalidateType.Nothing;

    public override string ToString() => _mask.ToString();
  }
}
