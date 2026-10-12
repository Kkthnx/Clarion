(function () {
  var root = document.documentElement;
  try { var saved = localStorage.getItem('theme'); if (saved) root.setAttribute('data-theme', saved); } catch (e) {}
  var themeButton = document.getElementById('theme');
  if (themeButton) themeButton.addEventListener('click', function () {
    var dark = root.getAttribute('data-theme') ? root.getAttribute('data-theme') === 'dark' : !window.matchMedia('(prefers-color-scheme: light)').matches;
    var next = dark ? 'light' : 'dark';
    root.setAttribute('data-theme', next);
    try { localStorage.setItem('theme', next); } catch (e) {}
  });

  var box = document.getElementById('q'), list = document.getElementById('results'), index = null, loading = null;
  function load() {
    if (index) return Promise.resolve(index);
    if (!loading) loading = fetch('assets/search-index.json').then(function (r) { return r.json(); }).then(function (rows) {
      rows.forEach(function (r) { r.h = (r.n + ' ' + r.s + ' ' + r.i).toLowerCase(); });
      index = rows; return rows;
    });
    return loading;
  }
  function show(rows, words) {
    list.textContent = '';
    if (!rows.length) { var p = document.createElement('p'); p.textContent = 'Nothing matches.'; list.appendChild(p); }
    rows.slice(0, 12).forEach(function (r) {
      var a = document.createElement('a'); a.href = r.u;
      a.textContent = r.n;
      var s = document.createElement('small'); s.textContent = r.g + '. ' + r.s;
      a.appendChild(s); list.appendChild(a);
    });
    list.hidden = false;
  }
  if (box) {
    box.addEventListener('input', function () {
      var words = box.value.toLowerCase().split(/\s+/).filter(Boolean);
      if (!words.length) { list.hidden = true; return; }
      load().then(function (rows) {
        var hits = rows.filter(function (r) { return words.every(function (w) { return r.h.indexOf(w) >= 0; }); });
        hits.sort(function (a, b) { return (b.n.toLowerCase().indexOf(words[0]) === 0) - (a.n.toLowerCase().indexOf(words[0]) === 0); });
        show(hits, words);
      });
    });
    box.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') { list.hidden = true; box.value = ''; }
      if (e.key === 'ArrowDown') { var first = list.querySelector('a'); if (first) { e.preventDefault(); first.focus(); } }
    });
    list.addEventListener('keydown', function (e) {
      var cur = document.activeElement;
      if (e.key === 'ArrowDown' && cur.nextElementSibling) { e.preventDefault(); cur.nextElementSibling.focus(); }
      if (e.key === 'ArrowUp') { e.preventDefault(); (cur.previousElementSibling || box).focus(); }
      if (e.key === 'Escape') { list.hidden = true; box.focus(); }
    });
    document.addEventListener('click', function (e) { if (!e.target.closest('.search')) list.hidden = true; });
    document.addEventListener('keydown', function (e) {
      if (e.key === '/' && !/INPUT|SELECT|TEXTAREA/.test(document.activeElement.tagName)) { e.preventDefault(); box.focus(); }
    });
  }

  var filter = document.getElementById('filter');
  if (filter) {
    var items = Array.prototype.slice.call(document.querySelectorAll('.item'));
    var topics = Array.prototype.slice.call(document.querySelectorAll('.topic'));
    var risk = document.getElementById('risk'), rec = document.getElementById('rec'), shown = document.getElementById('shown');
    function apply() {
      var words = filter.value.toLowerCase().split(/\s+/).filter(Boolean), n = 0;
      items.forEach(function (it) {
        var ok = words.every(function (w) { return it.getAttribute('data-text').indexOf(w) >= 0; }) &&
          (!risk.value || it.getAttribute('data-risk') === risk.value) && (!rec || !rec.value || it.getAttribute('data-rec') === rec.value);
        it.hidden = !ok; if (ok) n++;
      });
      topics.forEach(function (t) { t.hidden = !t.querySelector('.item:not([hidden])'); });
      shown.textContent = (filter.value || risk.value || (rec && rec.value)) ? 'Showing ' + n + ' of ' + items.length : '';
    }
    [filter, risk, rec].forEach(function (c) { if (c) c.addEventListener('input', apply); });
    if (location.hash) { var target = document.getElementById(decodeURIComponent(location.hash.slice(1))); if (target && target.tagName === 'ARTICLE') target.scrollIntoView(); }
  }
})();
