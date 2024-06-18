namespace pure.utils.task
{
  public struct InvalidateCache
  {
    private InvalidateType _mask;

    public InvalidateCache(InvalidateType val = InvalidateType.Nothing) => this._mask = val;

    public void Add(InvalidateType property = InvalidateType.All) => this._mask |= property;

    public bool Contains(InvalidateType property, InvalidateType args)
    {
      return this.Contains(property) || this.Contains(args);
    }

    public void Remove(InvalidateType property) => this._mask &= ~property;

    public bool Contains(InvalidateType property) => (this._mask & property) != 0;

    public bool Empty() => this._mask == InvalidateType.Nothing;

    public void Clear() => this._mask = InvalidateType.Nothing;

    public override string ToString() => this._mask.ToString();
  }
}
