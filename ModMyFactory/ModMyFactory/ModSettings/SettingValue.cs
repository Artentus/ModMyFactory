using System;

namespace ModMyFactory.ModSettings
{
    sealed class SettingValue
    {
        object value;

        public SettingType Type { get; }

        public object Value
        {
            get => value;
            set
            {
                if (value != this.value)
                {
                    try
                    {
                        switch (Type)
                        {
                            case SettingType.Boolean:
                                this.value = (bool)value;
                                break;
                            case SettingType.Integer:
                                this.value = (long)value;
                                break;
                            case SettingType.FloatingPoint:
                                this.value = (decimal)value;
                                break;
                            case SettingType.String:
                                this.value = (string)value;
                                break;
                        }
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new InvalidOperationException("Value was not of the correct type.", ex);
                    }
                }
            }
        }

        public bool GetBoolean()
        {
            if (Type != SettingType.Boolean) throw new InvalidOperationException("Wrong value type.");
            return (bool)Value;
        }

        public long GetInteger()
        {
            if (Type != SettingType.Integer) throw new InvalidOperationException("Wrong value type.");
            return (long)Value;
        }

        public decimal GetFloatingPoint()
        {
            if (Type == SettingType.FloatingPoint)
                return (decimal)Value;
            else if (Type == SettingType.Integer)
                return (decimal)(long)Value;
            else
                throw new InvalidOperationException("Wrong value type.");
        }

        public string GetString()
        {
            if (Type != SettingType.String) throw new InvalidOperationException("Wrong value type.");
            return (string)Value;
        }

        public void Set(bool value)
        {
            if (Type != SettingType.Boolean) throw new InvalidOperationException("Wrong value type.");
            this.value = value;
        }

        public void Set(long value)
        {
            if (Type != SettingType.Integer) throw new InvalidOperationException("Wrong value type.");
            this.value = value;
        }

        public void Set(decimal value)
        {
            if (Type != SettingType.FloatingPoint) throw new InvalidOperationException("Wrong value type.");
            this.value = value;
        }

        public void Set(string value)
        {
            if (Type != SettingType.String) throw new InvalidOperationException("Wrong value type.");
            this.value = value;
        }

        public SettingValue(object value, SettingType type)
        {
            Type = type;
            Value = value;
        }

        public SettingValue(bool value)
        {
            Type = SettingType.Boolean;
            this.value = value;
        }

        public SettingValue(long value)
        {
            Type = SettingType.Integer;
            this.value = value;
        }

        public SettingValue(decimal value)
        {
            Type = SettingType.FloatingPoint;
            this.value = value;
        }

        public SettingValue(string value)
        {
            Type = SettingType.String;
            this.value = value;
        }
    }
}
