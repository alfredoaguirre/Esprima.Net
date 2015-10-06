
function g() {
    alert(g.caller.name); // f
}

function f() {
    alert(f.caller.name); // undefined
    g();
}

function f1(data) {
    alert(data); // undefined
    g();
}
function f12(data, data2) {
    alert(data); // undefined
    g();
}

f();