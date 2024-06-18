namespace pure.ui {
	public interface IValueField<T> {
		void SetValue(T val);

		T GetValue();
	}
}