import numpy as np
import matplotlib.pyplot as plt

def generate_paths(times, N, hazard0, fx0, vol_haz, vol_fx, rho, jump):
    
    W = np.random.multivariate_normal([0,0], [[1, rho],[rho,1]],size=(N,len(times)))
    prob = np.zeros((N,))
    
    hazard = np.zeros((N, len(times)))
    hazard[:,0] = hazard0
    for i in range(1, len(times)):
        dt = times[i]-times[i-1]
        sdt = np.sqrt(dt)
        dW =  W[:,i-1,0]
        hazard[:,i] = hazard[:,i-1] * np.exp(-0.5*vol_haz*vol_haz*dt + vol_haz*sdt*dW)
    
    sp = np.fliplr(np.exp(-np.cumsum(hazard[:,0:-1]*dt, axis = 1)))
    u = np.random.uniform(size = (N,))
    
    tau = np.zeros((N,))
    tau_imp = np.zeros((N,))
    for i in range(len(u)):
        tau[i] = len(times)-np.searchsorted(sp[i,:],u[i])
        if (np.random.uniform()<0.5):
            u2 = np.random.uniform(sp[i,0], 1.0)
            tau_imp[i] = len(times)-np.searchsorted(sp[i,:],u2)
            prob[i] = (1-sp[i,0])/0.5
        else:
            u2 = np.random.uniform(0, sp[i,0])
            tau_imp[i] = len(times)-np.searchsorted(sp[i,:],u2)
            prob[i] = sp[i,0]/0.5
    
        
    fx = np.zeros((N, len(times)))
    fx_deval = np.zeros((N, len(times)))    
    fx_imp = np.zeros((N, len(times)))    
    fx[:,0] = fx0
    fx_deval[:,0] = fx0 * (1+jump)
    fx_imp[:,0] = fx0
    for i in range(1, len(times)):
        dt = times[i]-times[i-1]
        sdt = np.sqrt(dt)
        dW =  W[:,i-1,1]
        fx_deval[:,i] = fx_deval[:,i-1] * np.exp(-0.5*vol_fx*vol_fx*dt + vol_fx*sdt*dW)  
        fx_deval[:,i] = fx_deval[:,i]*np.exp(-hazard[:,i-1]*dt*np.log(1+jump))
        fx[:,i] = fx[:,i-1] * np.exp(-0.5*vol_fx*vol_fx*dt + vol_fx*sdt*dW)  
        fx[:,i] = fx[:,i]*(1+jump*(tau==i))
        fx[:,i] = fx[:,i]*np.exp(-hazard[:,i-1]*dt*np.log(1+jump)*(tau>i))
        fx_imp[:,i] = fx_imp[:,i-1] * np.exp(-0.5*vol_fx*vol_fx*dt + vol_fx*sdt*dW)  
        fx_imp[:,i] = fx_imp[:,i]*(1+jump*(tau_imp==i))
        fx_imp[:,i] = fx_imp[:,i]*np.exp(-hazard[:,i-1]*dt*np.log(1+jump)*(tau_imp>i))
    
       
    return hazard, fx, fx_imp, fx_deval, tau.astype(int), tau_imp.astype(int), prob
    
    
def cva_on_fwd(hazard, fx, fx_imp, fx_deval, tau, tau_imp, prob, times):
    """ calculate the CVA on an FX forward that ends at the end of times using
    two methods:
        * default weighting
        * default times
    """
    nsims = fx.shape[0]
    K=10
    usd = 1e6
    dt = times[1]-times[0]
    # hazard based
    sp = np.exp(-np.cumsum(hazard[:,0:-1]*dt, axis = 1))
    sp = np.hstack((np.ones((nsims,1)), sp))
    dp = -np.diff(sp, axis=1)      
    v_plus_deval = usd * (fx_deval[:,1:] - K)
    v_plus_deval[v_plus_deval<0] = 0    
    cva_haz = np.sum(dp * v_plus_deval) / nsims    

    # default based
    v_plus = usd * (fx[:,1:] - K)
    v_plus[v_plus<0] = 0
    cva_def = 0
    for i in range(len(tau)):
        if tau[i]<len(times):
            cva_def += v_plus[i, tau[i]-1]
    cva_def = cva_def / nsims
    
    # default based with importance    
    v_plus = usd * (fx_imp[:,1:] - K)
    v_plus[v_plus<0] = 0
    cva_def2 = 0
    for i in range(len(tau_imp)):
        if tau_imp[i]<len(times):
            cva_def2 += prob[i] * v_plus[i, tau_imp[i]-1]
    cva_def2 = cva_def2 / nsims
    
    return cva_haz, cva_def, cva_def2
    
    
def fva_on_fwd(hazard, fx, fx_imp, fx_deval, tau, tau_imp, prob, times):
    """ calculate the FVA on an FX forward
    """    
    nsims = fx.shape[0]
    K=10
    usd = 1e6
    spread = 0.02
    dt = times[1]-times[0]
    
    # hazard based
    sp = np.exp(-np.cumsum(hazard[:,0:-1]*dt, axis = 1))

    v_plus_deval = usd * (fx_deval[:,1:]/(1+jump) - K)
    v_plus_deval[v_plus_deval<0] = 0    
    fva_haz = np.sum(sp * v_plus_deval * spread * dt) / nsims    

    # default based
    v_plus = usd * (fx[:,1:] - K)
    v_plus[v_plus<0] = 0
    fva_def = 0
    for i in range(len(tau)):        
        fva_def += np.sum(v_plus[i, 0:tau[i]] * spread * dt)
    fva_def = fva_def / nsims
    
    # default based with importance    
    v_plus = usd * (fx_imp[:,1:] - K)
    v_plus[v_plus<0] = 0
    fva_def2 = 0
    for i in range(len(tau_imp)):
        fva_def2 += prob[i] * np.sum(v_plus[i, :tau_imp[i]]* spread * dt)
    fva_def2 = fva_def2 / nsims
    
    return fva_haz, fva_def, fva_def2    

def convergence():
    cva_haz_conv = []
    cva_def_conv = []
    cva_def2_conv = []
    x = []
    for i in range(20, 39):        
        N = int(2**(i/2))
        print(str(N) + " simulations" )
        x.append(N)
        hazard, fx, fx_imp, fx_deval, tau, tau_imp, prob = generate_paths(times, N, hazard0, fx0, vol_haz, vol_fx, rho, jump)
        cva_haz, cva_def, cva_def2 = cva_on_fwd(hazard, fx, fx_imp, fx_deval, tau, tau_imp, prob, times)
        cva_haz_conv.append(cva_haz)
        cva_def_conv.append(cva_def)
        cva_def2_conv.append(cva_def2)

    plt.figure()        
    plt.plot(x, cva_def_conv)
    plt.plot(x, cva_def2_conv)
    plt.plot(x, cva_haz_conv)    
    plt.legend(("default simulation", "default simulation with importance", "forward default prob weighting"))
    plt.title("CVA convergence")
    plt.xlabel("number of simulations")
    plt.ylabel("CVA")
    plt.show()                
        
    plt.figure()        
    plt.plot(x[0:16], cva_def_conv[0:16])
    plt.plot(x[0:16], cva_def2_conv[0:16])
    plt.plot(x[0:16], cva_haz_conv[0:16])    
    plt.legend(("default simulation", "default simulation with importance", "forward default prob weighting"))
    plt.title("CVA convergence")
    plt.xlabel("number of simulations")
    plt.ylabel("CVA")        
    plt.show()
        
    return x, cva_haz_conv, cva_def_conv, cva_def2_conv
    
    
    
if __name__ == "__main__":
    fx0 = 10
    hazard0 = 0.01
    N = 10000
    T = 1
    n_steps = 13
    times = np.linspace(0, T, n_steps)
    vol_haz = 0.4
    vol_fx = 0.15
    rho = 0.0
    jump = 0.3
    
    # Test call
    #hazard, fx, fx_imp, fx_deval, tau, tau_imp, prob = generate_paths(times, N, hazard0, fx0, vol_haz, vol_fx, rho, jump)
    #cva_haz, cva_def, cva_def2 = cva_on_fwd(hazard, fx, fx_imp, fx_deval, tau, tau_imp, prob, times)
    #fva_haz, fva_def, fva_def2 = fva_on_fwd(hazard, fx, fx_imp, fx_deval, tau, tau_imp, prob, times)

    # convergence call
    x, cva_haz_conv, cva_def_conv, cva_def2_conv = convergence();
