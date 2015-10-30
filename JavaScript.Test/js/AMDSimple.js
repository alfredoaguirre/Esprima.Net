define(function () {
    this.privateValue = 0;
    return {
        increment:function () {
            privateValue++;
        },

        decrement: function () {
            privateValue--;
        },

        getValue: function () {
            return privateValue;
        }
    };
});