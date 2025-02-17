namespace PredikceVytěžováníFVE.Models {
    public readonly struct TimeValuePair {
        public readonly DateTime Time;
        public readonly decimal Value;

        public TimeValuePair(DateTime time, decimal value) {
            Time = time;
            Value = value;
        }
    };
}
