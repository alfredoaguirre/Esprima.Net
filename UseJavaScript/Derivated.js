define([
'System',
'System.Collections.Generic',
'System.Linq',
'System.Text',
'System.Threading.Tasks',
'UseJavaScript.ToJs1' // BaseClass
], function (System, Generic, Linq, Text, Tasks, ToJs1) {
    return declare(ToJs1, {


        fun1: function () {

        },

        /** 
        * @param {string} srt
        * @return {objet}
        */
        fun2: function (srt) {
            typeof srt === 'string' || console.error("fun3 arg srt != {string}");
            return null;
        },

        /** 
        * @param {string} srt
        * @param {number} number
        * @return {objet}
        */
        fun3: function (srt, num) {
            (typeof srt === 'string') || console.error("fun3 arg srt != {string}");
            (typeof num === 'number') || console.error("fun3 arg num != {number}");
            return null;
        }
    });
});