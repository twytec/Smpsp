window.getStorage = (key) => {
    return localStorage.getItem(key);
}

window.setStorage = (key, data) => {
    localStorage.setItem(key, data);
}

window.deleteStorage = (key) => {
    localStorage.removeItem(key);
}