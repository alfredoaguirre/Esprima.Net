
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

f();