
function g() {
    alert(g.caller.name); // f
}

function f() {
    alert(f.caller.name); // undefined
    g();
}

f();