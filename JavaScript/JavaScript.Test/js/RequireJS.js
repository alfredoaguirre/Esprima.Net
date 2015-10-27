define(['lib/dependency1', 'lib/dependency2'], function (d1, d2) {

    //Your actual script goes here.   
    //The dependent scripts will be fetched if necessary.
        
    this.in = "in";
    this.out = "out";

    return this;  //For example, jQuery object
});