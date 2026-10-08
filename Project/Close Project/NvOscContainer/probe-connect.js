(function(){
  window.__c = 'pending';
  try {
    var i = angular.element(document.body).injector();
    var G = i.get('piplConfigService'), f = i.get('$rootScope');
    G.isConnectEnabled().then(function(v){
      window.__c = 'OK ' + JSON.stringify(v).slice(0,60);
      try { f.$apply(); } catch(e){}
    }, function(e){
      window.__c = 'REJ ' + String(e && (e.message || e.statusText || e)).slice(0,150);
      try { f.$apply(); } catch(e2){}
    });
    setTimeout(function(){ try { f.$apply(); } catch(e){} }, 500);
  } catch(e){ window.__c = 'THROW ' + e.message; }
})()
